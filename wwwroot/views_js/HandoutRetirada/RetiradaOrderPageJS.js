
console.log(RetiradaList);

document.addEventListener('DOMContentLoaded', function () {

    let inputElement = document.getElementById('finalRfid');
    inputElement.value = '';

    //inputElement.addEventListener('copy', e => e.preventDefault());
    //inputElement.addEventListener('cut', e => e.preventDefault());
    //inputElement.addEventListener('paste', e => e.preventDefault());

});

if (RetiradaList.length > 0) {
    console.log('Retirada is not empty');

    makeTableRetirada(RetiradaList);
}


function makeTableRetirada(Items) {

    const tbody = document.getElementById('RetiradaOrderBody');
    tbody.innerHTML = '';

    Items.forEach((item,index) => {

        const row = document.createElement('tr');
        row.id = `ReservedRow-${item.idReservation}`; 
        row.setAttribute("data-index", index);
        row.dataset.reservationId = item.idReservation; 
        row.dataset.transactionId = generateTransactionId();
        row.dataset.transferTransId = generateTransactionId();

        const orderNoCell = document.createElement('td');
        orderNoCell.className = 'text-center align-middle';
        orderNoCell.textContent = item.idReservationControl;
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdReservationControl`, item.idReservationControl));
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdReservation`, item.idReservation));

        const classeCell = document.createElement('td');
        classeCell.className = 'text-center align-middle';
        classeCell.textContent = item.classe;
        classeCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].Classe`, item.intClasse));

        const typeCell = document.createElement('td');
        typeCell.className = 'text-center align-middle';
        typeCell.textContent = item.type;
        typeCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].PorType`, item.type));

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = item.itemNome;

        const caCell = document.createElement('td');
        caCell.className = 'text-center align-middle';
        caCell.setAttribute("data-ExistCA", false);

        const inputCAHidden = document.createElement('input');
        inputCAHidden.type = 'hidden';
        inputCAHidden.id = `hiddenCA-${item.idReservation}`;
        inputCAHidden.name = `FinalProcessList[${index}].IdControleCA`;
       

        if (item.listCA.length > 1) {

            const openProductModal = document.createElement('a');
            openProductModal.className = 'btn btn-link';
            openProductModal.textContent = "Select CA";

            openProductModal.setAttribute("data-bs-toggle", "tooltip");
            openProductModal.setAttribute("data-bs-placement", "top");
            openProductModal.setAttribute("data-bs-custom-class", "custom-tooltip");
            openProductModal.setAttribute("data-bs-title", "Select CA");

            const tooltip = new bootstrap.Tooltip(openProductModal)

            openProductModal.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent default anchor behavior if needed
                openModalCA(item, row); // Call your function and pass the item
            });

            caCell.setAttribute("data-ExistCA", true);

            caCell.appendChild(openProductModal);

        } else if (item.listCA.length == 1) {


            caCell.textContent = item.listCA[0].numeroCA;
            caCell.setAttribute("data-bs-toggle", "tooltip");
            caCell.setAttribute("data-bs-placement", "top");
            caCell.setAttribute("data-bs-custom-class", "custom-tooltip");

            const formattedDate = new Date(item.listCA[0].validade)
                .toLocaleDateString('en-GB', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric'
                })
                .replace(/\//g, '-'); 

            caCell.setAttribute("data-bs-title", formattedDate);
            new bootstrap.Tooltip(caCell);

            inputCAHidden.value = item.listCA[0].id;


        } else if (item.listCA.length == 0) {

            caCell.textContent = '';

        }

        caCell.appendChild(inputCAHidden);
      

        const AfPatCell = document.createElement('td');

        const stockCell = document.createElement('td');
        stockCell.className = 'text-center align-middle';

        const qtyReqCell = document.createElement('td');
        qtyReqCell.className = 'text-center align-middle';

        const inputProdutoHidden = document.createElement('input');
        inputProdutoHidden.type = 'hidden';
        inputProdutoHidden.id = `hiddenProduto-${item.idReservation}`;
        inputProdutoHidden.name = `FinalProcessList[${index}].IdProduto`;

        if (item.listProducts.length > 1) {

            const openProductModal = document.createElement('a');
            openProductModal.className = 'btn btn-link';
            openProductModal.textContent = "Select AF/PAT";

            openProductModal.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent default anchor behavior if needed
                openModalFunction(item, row, index); // Call your function and pass the item
            });

            AfPatCell.appendChild(openProductModal);
            stockCell.textContent = "stock";
            qtyReqCell.textContent = item.quantidadeResquested;

        }
        else if (item.listProducts.length == 1) {

            AfPatCell.className = 'text-center align-middle';

            let afPart = '';
            let patPart = '';

            afPart = item.listProducts[0].af && item.listProducts[0].af.trim() !== '' ? `AF: ${item.listProducts[0].af}` : '';
            patPart = item.listProducts[0].pat !== null && item.listProducts[0].pat !== 0 ? patPart = `PAT: ${item.listProducts[0].pat}` : '';

            AfPatCell.textContent = [afPart, patPart].filter(part => part !== '').join(' ');
            stockCell.textContent = item.listProducts[0].stockQuantity;

            const inputQtyReq = document.createElement('input');
            inputQtyReq.type = 'number';
            inputQtyReq.className = 'form-control digitsonly';
            inputQtyReq.min = '1';
            inputQtyReq.step = '1';
            inputQtyReq.max = item.listProducts[0].stockQuantity.toString();
            inputQtyReq.value = item.quantidadeResquested.toString();
            inputQtyReq.name = `FinalProcessList[${index}].QtyRequested`;

            inputQtyReq.addEventListener('change', (e) => {

                let inputValue = Number(e.target.value);

                if (!inputValue) {
                    appendAlert(`The inputted QtyRequested for cannot be 0 or empty ${item.codigo}`, 'warning');
                    e.target.value = 1;
                    return;
                }

                if (inputValue > Number(item.listProducts[0].stockQuantity)) {
                    appendAlert(`The inputted QtyRequested for cannot be greater than the Stock Quantity for ${item.codigo}`, 'warning');
                    e.target.value = 1;
                    return;
                }

            });

            inputProdutoHidden.value = item.listProducts[0].idProduto;

            qtyReqCell.appendChild(inputQtyReq);


        } else {
            AfPatCell.className = 'text-center align-middle';
            AfPatCell.textContent = "no product available";
            stockCell.textContent = "empty";
            qtyReqCell.textContent = item.quantidadeResquested;
        }

        AfPatCell.appendChild(inputProdutoHidden);

        const retornoCell = document.createElement('td');
        retornoCell.className = 'text-center align-middle';
        const inputRetorno = document.createElement('input');
        inputRetorno.type = 'date';
        inputRetorno.className = 'form-control';
        inputRetorno.name = `FinalProcessList[${index}].DateReturn`;

        if (item.dataReturn !== null && item.dataReturn !== "") {
            console.log('date', item.dataReturn)
            // Convert the datetime to the format required by date inputs (YYYY-MM-DD)
            const dateObj = new Date(item.dataReturn);

            // Get the date in YYYY-MM-DD format
            const year = dateObj.getFullYear();
            const month = String(dateObj.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
            const day = String(dateObj.getDate()).padStart(2, '0');

            const formattedDate = `${year}-${month}-${day}`;
            inputRetorno.value = formattedDate;
        } else {
            console.log('no date');
        }

        inputRetorno.addEventListener('change', (e) => {
            const selectedDate = new Date(e.target.value);
            const today = new Date();

            // Set time of both to midnight to compare only date part
            selectedDate.setHours(0, 0, 0, 0);
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                appendAlert(`The selected date cannot be earlier than today for ${item.codigo}.`, 'warning');
                e.target.value = ''; // Optional: reset the invalid date
            }
        });
        retornoCell.appendChild(inputRetorno);

        const solicitanteCell = document.createElement('td');
        solicitanteCell.className = 'text-center align-middle';
        solicitanteCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].ChapaSolicitante`, item.memberInfo.chapa));
        solicitanteCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].CodColigadaSolicitante`, item.memberInfo.codColigada));
        solicitanteCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdTerceiroSolicitante`, 0));

        const popoverLink = document.createElement("a");
        popoverLink.setAttribute("href", "#");
        popoverLink.textContent = item.memberInfo.chapa;

        popoverLink.onclick = function (e) {
            e.preventDefault();

            openEmployeeInformation(item.memberInfo, 'Solicitante');
        };

        solicitanteCell.appendChild(popoverLink);

        const liberadorCell = document.createElement('td');
        liberadorCell.className = 'text-center align-middle';
        liberadorCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].ChapaLiberador`, item.leaderInfo.chapa));
        liberadorCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].CodColigadaLiberador`, item.leaderInfo.codColigada));
        liberadorCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdTerceiroLiberador`, 0));

        const liberadorLink = document.createElement("a");
        liberadorLink.setAttribute("href", "#");
        liberadorLink.textContent = item.leaderInfo.chapa;
        liberadorLink.onclick = function (e) {
            e.preventDefault();

            openEmployeeInformation(item.leaderInfo, 'Liberador');
        };
        liberadorCell.appendChild(liberadorLink);

        const obsCell = document.createElement('td');
        obsCell.className = 'text-center align-middle';
        const inputObs = document.createElement('input');
        inputObs.className = 'form-control';
        inputObs.name = `FinalProcessList[${index}].Observacao`;
        obsCell.appendChild(inputObs);

        const cancelCell = document.createElement('td');
        cancelCell.className = 'text-center align-middle';
        cancelCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdObra`, item.idObra));

        const cancelButton = document.createElement('button');
        cancelButton.className = 'btn btn-danger btnCancel';
        cancelButton.type = 'button';
        cancelButton.textContent = 'Cancelar';
        cancelButton.addEventListener('click', function () {
            // Show confirmation dialog
            //const isConfirmed = confirm('Are you sure you want to cancel this?');

            //if (isConfirmed) {

            //    /*row.remove();*/
            //    cancelTransaction(row, item.idReservation);

            //    console.log('Cancellation confirmed');

            //}

            let cancelObservacao = prompt("Por favor, justifique o cancelamento desta reserva:");
            if (cancelObservacao) {
                cancelTransaction(row, item.idReservation, cancelObservacao);
            } else {
                appendAlertWithoutAnimation("Nenhum Observação de Cancellamento inserido.", 'danger');
            }

        });

        cancelCell.appendChild(cancelButton);

        if (item.isTransferable) {

            const transferButton = document.createElement('button');
            transferButton.className = 'btn btn-warning ms-1 text-white btnTransfer';
            transferButton.type = 'button';
            transferButton.textContent = 'Transferir';

            transferButton.addEventListener('click', async function () {

                const listFerramentaria = await getTransferList(item.idCatalogo);
                if (!listFerramentaria || listFerramentaria.length === 0) {
                    appendAlert('Não há ferramentaria disponível para transferência', 'danger');
                    return;
                }

                //if (listFerramentaria.length === 0) {
                //    appendAlert('Não há ferramentaria disponível para transferência', 'danger');
                //}

                openTransferModal(row, listFerramentaria, item);

                /*console.log(listFerramentaria);*/

            });

            cancelCell.appendChild(transferButton);

        }

      


        row.appendChild(orderNoCell);
        row.appendChild(classeCell);
        row.appendChild(typeCell);
        row.appendChild(codigoCell);
        row.appendChild(nomeCell);
        row.appendChild(caCell);
        row.appendChild(AfPatCell);     
        row.appendChild(stockCell);
        row.appendChild(qtyReqCell);
        row.appendChild(retornoCell);
        row.appendChild(solicitanteCell);
        row.appendChild(liberadorCell);
        row.appendChild(obsCell);
        row.appendChild(cancelCell);
        tbody.appendChild(row);
    });
}


function generateTransactionId() {
    if (crypto.randomUUID) {
        return crypto.randomUUID(); // Returns standard UUID v4
    }
    // Fallback for older browsers
    return generateGuidFallback();
}


const getTransferList = async (idCatalogo) => {

    let cleanup;
    try {

        cleanup = loaderButtons('Obtendo ferramentaria disponível...');

        const url = `/HandoutRetirada/GetFerramentariaList?IdCatalogo=${idCatalogo}`;

        const ferramentariaList = await fetchJson(url);

        if (cleanup) cleanup();
        return ferramentariaList;

        //const response = await fetch(url, { method: 'GET' });

        //if (!response.ok) {
        //    throw new Error(`HTTP error! Status: ${response.status}`);
        //}

        //const data = await response.json();

        //if (data.success) {
        //    return data.listFerramentaria;
        //} else {
        //    appendAlert(data.message, 'danger');
        //}


    } catch (error) {

        if (cleanup) cleanup();

        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

        return null;

    }

}




const openTransferModal = (row, listFerramentaria, detail) => {

    const modal = new bootstrap.Modal(document.getElementById('transferModal'));
    const modalElement = document.getElementById('transferModal');

    const tableBody = modalElement.querySelector("#reservationTransferBody");
    tableBody.innerHTML = '';

    const transferRow = document.createElement('tr');
    transferRow.dataset.reservationId = detail.idReservation; 
    transferRow.dataset.transferTransId = row.dataset.transferTransId; 

    const orderNoCell = document.createElement('td');
    orderNoCell.className = 'text-center align-middle';
    orderNoCell.textContent = detail.idReservation;

    const codigoCell = document.createElement('td');
    codigoCell.className = 'text-center align-middle';
    codigoCell.textContent = detail.codigo;

    const nameCell = document.createElement('td');
    nameCell.className = 'text-center align-middle';
    nameCell.textContent = detail.itemNome;

    transferRow.appendChild(orderNoCell);
    transferRow.appendChild(codigoCell);
    transferRow.appendChild(nameCell);

    tableBody.appendChild(transferRow);

    const subDropdown = document.getElementById("transferSelect");

    listFerramentaria.forEach(item => {
        const option = document.createElement("option");
        option.value = item.id;
        option.textContent = item.nome;
        subDropdown.appendChild(option);
    });


    modal.show();


}

document.getElementById("btnTransfer").addEventListener("click", async (e) => {

    const transferBtn = e.target;
    const modalElement = document.getElementById('transferModal');
    const closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"], .btn-close');

    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

    let cleanup;

    try {

        cleanup = openTransferTransactionLoader(transferBtn, modal, closeButtons);

        const orderNo = modalElement.querySelector("#reservationTransferBody tr").dataset.reservationId;
        if (!orderNo) throw new Error('Numero do Pedido está vazio');   

        const IdFerramentaria = modalElement.querySelector('#transferSelect').value;
        if (!IdFerramentaria) throw new Error('Ferramentaria para transferir está vazia');

        const Observacao = modalElement.querySelector('#inputObs').value;
        if (!Observacao) throw new Error('Observacao está vazio');

        if (Observacao.length > 80) throw new Error('A Observacao é muito longa, limite-a a 80 caracteres.');

        const transactionId = modalElement.querySelector("#reservationTransferBody tr").dataset.transferTransId;
        if (!transactionId) throw new Error('Numero do Pedido está vazio');   

        const url = `/HandoutRetirada/TransferReservation`;

        const formData = new FormData();
        formData.append('OrderNo', orderNo);
        formData.append('IdFerramentaria', IdFerramentaria);
        formData.append('Observacao', Observacao);
        formData.append('IdTransaction', transactionId);

        const result = await fetchJson(url, {
            method: 'POST',
            body: formData,
            headers: {
                /*'Content-Type': 'multipart/form-data',*/
                'Accept': 'application/json'
            }
        });

        if (!result.success) {
            throw new Error(result.message);
        }

        if (cleanup) cleanup();
       
        const matchingRow = document.querySelector(`#RetiradaOrderBody tr[data-reservation-id="${orderNo}"]`);
        matchingRow.remove();
        reindexFormRows();
        modal.hide();

        appendAlert(result.message, 'success');

    } catch (error) {

        if (cleanup) cleanup();
        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

    } finally {
        if (cleanup) cleanup(); // Always runs
    }



    ///*const modal = new bootstrap.Modal(document.getElementById('transferModal'));*/
    //const modalElement = document.getElementById('transferModal');
    //const modal = bootstrap.Modal.getInstance(document.getElementById('transferModal')) || new bootstrap.Modal(document.getElementById('transferModal'));

    ///*const orderNo = modal.querySelector("#reservationTransferBody tr td:nth-child(1)").textContent.trim();*/
    //const orderNo = modalElement.querySelector("#reservationTransferBody tr").dataset.reservationId;
    //if (!orderNo) {
    //    appendAlert('reservationId está vazio', 'danger');
    //    return;
    //}
    //console.log(orderNo);

    //const IdFerramentaria = modalElement.querySelector('#transferSelect').value;
    //if (!IdFerramentaria) {
    //    appendAlert('IdFerramentaria está vazio', 'danger');
    //    return;
    //}
    //console.log(IdFerramentaria);

    //const Observacao = modalElement.querySelector('#inputObs').value;
    //if (!Observacao) {
    //    appendAlert('Observacao está vazio', 'danger');
    //    return;
    //}

    //if (Observacao.length > 80) {
    //    appendAlert('A Observacao é muito longa, limite-a a 80 caracteres.', 'danger');
    //    return;
    //}

    //console.log(Observacao);

    //const formData = new FormData();
    //formData.append('OrderNo', orderNo);
    //formData.append('IdFerramentaria', IdFerramentaria);
    //formData.append('Observacao', Observacao);

    //const response = await fetch('/HandoutRetirada/TransferReservation', {
    //    method: 'POST',
    //    body: formData,
    //});

    //if (!response.ok) {
    //    throw new Error(`HTTP error! Status: ${response.status}`);
    //}

    //const data = await response.json();

    //if (data.success) {

    //    const matchingRow = document.querySelector(`#RetiradaOrderBody tr[data-reservation-id="${orderNo}"]`);
    //    if (matchingRow) {
    //        matchingRow.remove();
    //        modal.hide();
    //        appendAlert(data.message, 'success');
    //    } else {
    //        appendAlert("No matching row found", 'danger');
    //        console.log("No matching row found");
    //    }

    //} else {
    //    appendAlert(data.message, 'danger');  // Show error message
    //    /*throw new Error(data.message); */
    //}


});


const openTransferTransactionLoader = (btn, modal, closeButtons) => {

    const modalElement = modal._element;

    const preventHide = (e) => {
        e.preventDefault();
        return false;
    };

    modalElement.addEventListener('hide.bs.modal', preventHide);
    closeButtons.forEach(btn => btn.style.pointerEvents = 'none');
    btn.disabled = true;
    btn.querySelector('#loadingConfirmation').classList.remove('visually-hidden');
    document.getElementById('transferSelect').style.pointerEvents = 'none';
    document.getElementById('transferSelect').style.backgroundColor = '#f8f9fa';
    document.getElementById('inputObs').readOnly = true;

    return () => {
        modalElement.removeEventListener('hide.bs.modal', preventHide);
        closeButtons.forEach(btn => btn.style.pointerEvents = 'auto');
        btn.disabled = false;
        btn.querySelector('#loadingConfirmation').classList.add('visually-hidden');
        document.getElementById('transferSelect').style.pointerEvents = '';
        document.getElementById('transferSelect').style.backgroundColor = '';
        document.getElementById('inputObs').readOnly = false;
    };

}



const cancelTransaction = async (row, IdReservation, observacao) => {

    let cleanup;

    cleanup = loaderButtons(`Item: ${row.cells[3].textContent} - Cancelando...`);


    try {

        const url = `/HandoutRetirada/CancelTransaction`;

        const formData = new URLSearchParams();
        formData.append('IdReservation', IdReservation);
        formData.append('Observacao', observacao);
        formData.append('IdTransaction', row.dataset.transactionId);

        const result = await fetchJson(url, {
            method: 'POST',
            body: formData,
            headers: {
                /*'Content-Type': 'multipart/form-data',*/
                'Accept': 'application/json'
            }
        });

        if (!result.success) {
            throw new Error(result.message);
        }

        row.remove();
        reindexFormRows();
        appendAlert(result.message, 'success');

        if (cleanup) cleanup();

    } catch (error) {

        if (cleanup) cleanup();

        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

    }




    //$.ajax({
    //    url: '/HandoutRetirada/CancelTransaction',
    //    type: 'POST',
    //    data: { IdReservation: IdReservation, Observacao: observacao },
    //    success: function (data) {
    //        if (data.success) {

    //            appendAlert(data.message, 'success');
    //            row.remove();

                

    //        } else {
    //            appendAlert(data.message, 'danger');
    //        }
    //    },
    //    error: function (error) {
    //        console.error('Error fetching items:', error);
    //        appendAlertWithoutAnimation(error, 'danger');
    //    }
    //});
}

function reindexFormRows() {
    const rows = document.querySelectorAll('#RetiradaOrderBody tr');

    rows.forEach((row, newIndex) => {
        // Update ALL inputs that start with "FinalProcessList" in this row
        const allInputs = row.querySelectorAll('input[name^="FinalProcessList"]');

        allInputs.forEach(input => {
            const oldName = input.name;
            // Extract the property name from the old name
            const propertyMatch = oldName.match(/FinalProcessList\[\d+\]\.(.+)/);
            if (propertyMatch) {
                const propertyName = propertyMatch[1];
                input.name = `FinalProcessList[${newIndex}].${propertyName}`;
            }
        });

        // Update data attributes if needed
        row.dataset.index = newIndex;
    });
}


function openModalFunction(item, productRow, index) {

    console.log(item);
    // Get the modal element (assuming you're using Bootstrap)
    const modal = new bootstrap.Modal(document.getElementById('exampleModal'));

    const modalElement = document.getElementById('exampleModal');
    modalElement.querySelector('#modalTitle').textContent = "";
    modalElement.querySelector('#modalTitle').textContent = `Order No:${item.idReservationControl} - Codig:${item.codigo}`;

    const modalBody = modalElement.querySelector('#tableBodyDiv');
    modalBody.innerHTML = "";

    const ProductTable = document.createElement('table');
    ProductTable.className = "table";

    const ProductHead = document.createElement('thead');
    const tr = document.createElement('tr');

    const th = document.createElement('th');
    th.className = "text-center align-middle";
    th.textContent = "";

    const th1 = document.createElement('th');
    th1.className = "text-center align-middle";
    th1.textContent = "AF";

    const th2 = document.createElement('th');
    th2.className = "text-center align-middle";
    th2.textContent = "PAT";

    const th3 = document.createElement('th');
    th3.className = "text-center align-middle";
    th3.textContent = "";


    tr.append(th, th1, th2, th3);
    ProductHead.appendChild(tr);
    ProductTable.appendChild(ProductHead);

    const ProductBody = document.createElement('tbody');
    ProductBody.id = "AfPatBody";
    item.listProducts.forEach(product => {

        const row = document.createElement('tr');

        const selectCell = document.createElement('td');
        selectCell.className = "text-center align-middle";

        const afCell = document.createElement('td');
        afCell.className = "text-center align-middle afcell";
        afCell.textContent = product.af;

        const patCell = document.createElement('td');
        patCell.className = "text-center align-middle patcell";
        patCell.textContent = product.pat;

        if (product.allowedToBorrow) {
            const selectProduct = document.createElement('a');
            selectProduct.className = 'btn btn-link';
            selectProduct.textContent = "Select";

            // On click: Update main table's stock cell and close modal
            selectProduct.addEventListener('click', () => {
                // Update the main table's stock cell (caCell)

                //const caCell = productRow.querySelector('td:nth-child(8)'); // 4th cell (caCell)
                //caCell.textContent = product.stockQuantity; // Assuming `stockQuantity` exists

                const AfPatCell = productRow.querySelector('td:nth-child(7)');
                const existingButton = AfPatCell.querySelector('a.btn-link');

                const existingHiddenInput = AfPatCell.querySelector(`#hiddenProduto-${item.idReservation}`);
                existingHiddenInput.value = product.idProduto;

                if (existingButton) {
                    existingButton.textContent = `AF: ${product.af} PAT: ${product.pat}`;
                }

                const qtyStockCell = productRow.querySelector('td:nth-child(8)');
                qtyStockCell.textContent = product.stockQuantity;

                const qtyReqCell = productRow.querySelector('td:nth-child(9)');
                qtyReqCell.textContent = '';

                const input = document.createElement('input');
                input.type = 'number';
                input.className = 'form-control digitsonly';
                input.min = '1';
                input.step = '1';
                input.max = product.stockQuantity.toString();
                input.value = item.quantidadeResquested.toString();
                /*input.name = `FinalProcessList[${index}].QtyRequested`;*/

                console.log('item check', item)
                if (item.type === "PorAferido" || item.type === "PorSerial") {
                    input.disabled = true;
                    qtyStockCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].QtyRequested`, 1));
                }

                input.addEventListener('change', (e) => {

                    let inputValue = Number(e.target.value);

                    console.log('changed', inputValue);

                    console.log('stock', product.stockQuantity);

                    if (!inputValue) {
                        appendAlert(`The inputted QtyRequested for cannot be 0 or empty ${item.codigo}`, 'warning');
                        e.target.value = 1;
                        return;
                    }

                    if (inputValue > Number(product.stockQuantity)) {
                        appendAlert(`The inputted QtyRequested for cannot be greater than the Stock Quantity for ${item.codigo}`, 'warning');
                        e.target.value = 1;
                        return;
                    }

                });

                qtyReqCell.replaceChildren(input);
                /*qtyReqCell.appendChild(input);*/


                // Close the modal
                modal.hide();
            });

            selectCell.appendChild(selectProduct);
        } else {
            row.classList.add('table-danger');
        }

        const reasonCell = document.createElement('td');
        reasonCell.className = "text-center align-middle";
        reasonCell.textContent = product.reason;

        row.append(selectCell, afCell, patCell, reasonCell);
        ProductBody.appendChild(row);

    });

    ProductTable.appendChild(ProductBody);
    modalBody.appendChild(ProductTable);




    // Here you can populate the modal with data from the item
    // For example:
    // document.getElementById('modal-title').textContent = item.name;
    // document.getElementById('modal-body').textContent = item.description;

    // Show the modal
    modal.show();
}

function openModalCA(item, productRow) {

    const modal = new bootstrap.Modal(document.getElementById('exampleModal'));

    const modalElement = document.getElementById('exampleModal');
    modalElement.querySelector('#modalTitle').textContent = "";
    modalElement.querySelector('#modalTitle').textContent = `Order No:${item.idReservationControl} - Codig:${item.codigo}`;

    const modalBody = modalElement.querySelector('#modalBody');
    modalBody.innerHTML = "";

    const CATable = document.createElement('table');
    CATable.className = "table";

    const tableHead = document.createElement('thead');
    const tr = document.createElement('tr');

    const th = document.createElement('th');
    th.className = "text-center align-middle";
    th.textContent = "";

    const th1 = document.createElement('th');
    th1.className = "text-center align-middle";
    th1.textContent = "Numero CA";

    const th2 = document.createElement('th');
    th2.className = "text-center align-middle";
    th2.textContent = "Validade";

    tr.append(th, th1, th2);
    tableHead.appendChild(tr);
    CATable.appendChild(tableHead);

    const CABody = document.createElement('tbody');

    item.listCA.forEach(ca => {

        const row = document.createElement('tr');

        const selectCell = document.createElement('td');
        selectCell.className = "text-center align-middle";

        const selectCA = document.createElement('a');
        selectCA.className = 'btn btn-link';
        selectCA.textContent = "Select";

        const formattedDate = new Date(ca.validade)
            .toLocaleDateString('en-GB', {  // Use British format (day-month-year)
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            })
            .replace(/\//g, '-'); 

        selectCA.addEventListener('click', () => {

            const caCell = productRow.querySelector('td:nth-child(6)'); 

            const existingButton = caCell.querySelector('a.btn-link');

            if (existingButton) {
                existingButton.textContent = ca.numeroCA;
            }

            const existingHiddenInput = caCell.querySelector(`#hiddenCA-${item.idReservation}`);
            existingHiddenInput.value = ca.id;

            existingButton.setAttribute("data-bs-title", formattedDate);
            existingButton.setAttribute("data-bs-toggle", "tooltip");
            existingButton.setAttribute("data-bs-placement", "top");
            existingButton.setAttribute("data-bs-custom-class", "custom-tooltip");

            const tooltipInstance = bootstrap.Tooltip.getInstance(existingButton);
            if (tooltipInstance) {
                tooltipInstance.dispose();
            }

            const tooltip = new bootstrap.Tooltip(existingButton)

            //const tooltipInstance = bootstrap.Tooltip.getInstance(existingButton);
            //if (tooltipInstance) {
            //    tooltipInstance.setContent({ '.tooltip-inner': formattedDate });
            //}

            modal.hide();

        });

        selectCell.appendChild(selectCA);

        const numeroCell = document.createElement('td');
        numeroCell.className = "text-center align-middle";
        numeroCell.textContent = ca.numeroCA;

        const dateCell = document.createElement('td');
        dateCell.className = "text-center align-middle";


        dateCell.textContent = formattedDate;

        row.append(selectCell, numeroCell, dateCell);
        CABody.appendChild(row);
    });

    CATable.appendChild(CABody);
    modalBody.appendChild(CATable);

    modal.show();

}

function openEmployeeInformation(employee, titleName) {

    const modal = new bootstrap.Modal(document.getElementById('popoverModal'));

    const modalElement = document.getElementById('popoverModal');

    modalElement.querySelector('#modalEmployeeTitle').textContent = "";
    modalElement.querySelector('#modalEmployeeTitle').textContent = `${titleName}`;

    //if (employee.imagebase64 !== null) {
    //    modalElement.querySelector('#imgEmployee').src = employee.imageStringByte;
    //} else {
    //    modalElement.querySelector('#imgEmployee').src = "/images/image-not-available.jpg";
    //}

    modalElement.querySelector('#chapaLabel').textContent = employee.chapa;
    modalElement.querySelector('#nomeLabel').textContent = employee.nome;

    modalElement.querySelector('#situacaoLabel').textContent = "";
    modalElement.querySelector('#situacaoLabel').textContent = employee.codSituacao;

    modalElement.querySelector('#funcaoLabel').textContent = "";
    modalElement.querySelector('#funcaoLabel').textContent = employee.funcao;

    modalElement.querySelector('#secaoLabel').textContent = "";
    modalElement.querySelector('#secaoLabel').textContent = employee.secao;


    modal.show();
}

function HiddenInputProcess(name, value) {
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = name;
    input.value = value;
    return input;
}

let rfidTimeout;

window.addEventListener('beforeunload', () => {
    clearTimeout(rfidTimeout);
});


document.getElementById("finalRfid").addEventListener("input", async (event) => {

    clearTimeout(rfidTimeout);

    rfidTimeout = setTimeout(async () => {

        let cleanup
        try {

            const errors = [];

            cleanup = loaderButtons('finalizando transação...');

            if (!event.target.value.trim()) throw new Error('Por favor, insira o número do crachá.');

            const rows = document.querySelectorAll('#RetiradaOrderBody tr');
            if (rows.length === 0) throw new Error('Não existem itens para finalizar a transação.');

            rows.forEach((row) => {

                const sample = row.dataset.index;

                const codigo = row.querySelector('td:nth-child(4)').textContent;

                const input = row.querySelector(`input[type="hidden"][name="FinalProcessList[${sample}].IdProduto"]`);
                if (!input || !input.value) {
                    row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                    errors.push(`${codigo} doesnt have IdProduto Selected`);
                }

                const inputQty = row.querySelector(`input[name="FinalProcessList[${sample}].QtyRequested"]`);
                if (!inputQty || !inputQty.value) {
                    row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                    errors.push(`${codigo} doesnt have Qty Inputted`);
                }

                const multipleCA = row.querySelector('td:nth-child(6)').dataset.existca;
                console.log(multipleCA);
                if (multipleCA === "true") {
                    const inputCA = row.querySelector(`input[name="FinalProcessList[${sample}].IdControleCA"]`);
                    console.log(inputCA);
                    if (!inputCA || !inputCA.value) {
                        row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                        errors.push(`${codigo} doesnt have CA selected`);
                    }
                }
            });

            if (errors.length > 0) {
                throw new Error(errors.join('<br>'));
            }

            var form = document.getElementById('FormFinalizeRetirada');

            const formData = new FormData(form);

            const result = await fetchJson(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    /*'Content-Type': 'multipart/form-data',*/
                    'Accept': 'application/json'
                }
            });

            if (!result.success) {
                throw new Error(result.message);
            }


            appendAlert(result.message, 'success');

            setTimeout(function () {
                window.location.href = `/HandoutRetirada/Index`;
            }, 2000);


        } catch (error) {

            if (cleanup) cleanup();
            console.error(error);
            if (error.status === 401 || error.status === 403) {
                handleAuthenticationError(error);
            } else {
                appendAlertWithoutAnimation(error.message, 'danger');
            }

        }

    }, 150);


    //event.preventDefault();

    //if (event.target.value !== "") {
    //    formSubmissionReservation();
    //    console.log('Form submitted!');
    //} else {
    //    appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
    //}


    //console.log('Form submitted!');

    //var form = document.getElementById('FormFinalizeRetirada');

    //form.submit();

});


//document.getElementById("finalRfid").addEventListener("change", function (event) {

//    event.preventDefault();

//    var form = document.getElementById('FormFinalizeRetirada');

//    const inputCracha = document.getElementById('finalRfid');

//    var isValid = true;

//    var crachaInput = event.target.value;
//    console.log(crachaInput);
//    if (!crachaInput) {
//        isValid = false;
//        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
//        event.preventDefault();
//        return;
//    }

//    const tbody = document.getElementById('RetiradaOrderBody');
//    const rows = tbody.querySelectorAll('tr');

//    console.log('row count', rows.length)

//    if (rows.length > 0) {

//        rows.forEach((row) => {

//            const sample = row.dataset.index;
//            console.log(sample);

//            const codigo = row.querySelector('td:nth-child(4)').textContent;
//            console.log(codigo);

//            const input = row.querySelector(`input[type="hidden"][name="FinalProcessList[${sample}].IdProduto"]`);
//            if (!input || !input.value) {
//                console.log('no produto selected');
//                appendAlertWithoutAnimation(`${codigo} doesnt have IdProduto Selected`, 'warning');
//                row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
//                isValid = false;
//                inputCracha.value = "";
//            }

//            const inputQty = row.querySelector(`input[name="FinalProcessList[${sample}].QtyRequested"]`);
//            if (!inputQty || !inputQty.value) {
//                console.log('no Qty inputted');
//                appendAlertWithoutAnimation(`${codigo} doesnt have Qty Inputted`, 'warning');
//                row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
//                isValid = false;
//                inputCracha.value = "";
//            }

//            const multipleCA = row.querySelector('td:nth-child(6)').dataset.existca;
//            console.log(multipleCA);
//            if (multipleCA === "true") {
//                const inputCA = row.querySelector(`input[name="FinalProcessList[${sample}].IdControleCA"]`);
//                console.log(inputCA);
//                if (!inputCA || !inputCA.value) {

//                    console.log('no ca');
//                    appendAlertWithoutAnimation(`${codigo} doesnt have CA selected`, 'warning');
//                    row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
//                    isValid = false;
//                    inputCracha.value = "";
//                }
//            }

//            return;
//        });

//    } else {
//        // No rows found
//        console.log('No rows found in table');
//        isValid = false;
//        appendAlertWithoutAnimation('No rows found in table', 'warning');
//        return;
//    }


//    if (isValid) {
//        form.submit();
//    }

//});


//document.getElementById("finalRfid").addEventListener("change", function (event) {

//    event.preventDefault();

//    if (event.target.value !== "") {
//        formSubmissionReservation();
//       console.log('Form submitted!');
//    } else {
//       appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
//    }


//    //console.log('Form submitted!');

//    //var form = document.getElementById('FormFinalizeRetirada');

//    //form.submit();

//});



//document.getElementById('FormFinalizeRetirada').addEventListener('submit', function (event) {
//    event.preventDefault(); // Prevent default form submission

//    console.log('Form submitted!');

//    let inputCracha = document.getElementById('finalRfid').value;

//    if (inputCracha !== "") {
//        formSubmissionAction();
//        console.log('Form submitted!');
//    } else {
//        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
//    }

  
//});

function formSubmissionReservation() {
    var form = document.getElementById('FormFinalizeRetirada');

    const inputCracha = document.getElementById('finalRfid');

    var isValid = true;

    var crachaInput = inputCracha.value;
    console.log(crachaInput);
    if (!crachaInput) {
        isValid = false;
        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
        return;
    }

    const tbody = document.getElementById('RetiradaOrderBody');
    const rows = tbody.querySelectorAll('tr');

    console.log('row count', rows.length)

    if (rows.length > 0) {

        rows.forEach((row) => {

            const sample = row.dataset.index;
            console.log(sample);

            const codigo = row.querySelector('td:nth-child(4)').textContent;
            console.log(codigo);

            const input = row.querySelector(`input[type="hidden"][name="FinalProcessList[${sample}].IdProduto"]`);
            if (!input || !input.value) {
                console.log('no produto selected');
                appendAlertWithoutAnimation(`${codigo} doesnt have IdProduto Selected`, 'warning');
                row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                isValid = false;
                inputCracha.value = "";
            }

            const inputQty = row.querySelector(`input[name="FinalProcessList[${sample}].QtyRequested"]`);
            if (!inputQty || !inputQty.value) {
                console.log('no Qty inputted');
                appendAlertWithoutAnimation(`${codigo} doesnt have Qty Inputted`, 'warning');
                row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                isValid = false;
                inputCracha.value = "";
            }

            const multipleCA = row.querySelector('td:nth-child(6)').dataset.existca;
            console.log(multipleCA);
            if (multipleCA === "true") {
                const inputCA = row.querySelector(`input[name="FinalProcessList[${sample}].IdControleCA"]`);
                console.log(inputCA);
                if (!inputCA || !inputCA.value) {

                    console.log('no ca');
                    appendAlertWithoutAnimation(`${codigo} doesnt have CA selected`, 'warning');
                    row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                    isValid = false;
                    inputCracha.value = "";
                }
            }

            return;
        });

    } else {
        // No rows found
        console.log('No rows found in table');
        isValid = false;
        appendAlertWithoutAnimation('No rows found in table', 'warning');
        return;
    }


    if (isValid) {
        form.submit();
    }
}


function goBackToIndex() {

    window.location.href = `/HandoutRetirada/Index`;
}



document.getElementById("inputSearchAF").addEventListener("keyup", (e) => {

    let inputtedValue = e.target.value.toUpperCase();

    var table = document.getElementById("AfPatBody");
    var tr = table.getElementsByTagName("tr");

    for (var i = 0; i < tr.length; i++) {

        var chapaTd = tr[i].getElementsByClassName("afcell")[0];
        /*var nomeTd = tr[i].getElementsByClassName("nomeemployee")[0];*/

        var chapaTxtValue = chapaTd ? (chapaTd.textContent || chapaTd.innerText) : "";
        /*var nomeTxtValue = nomeTd ? (nomeTd.textContent || nomeTd.innerText) : "";*/

        if (chapaTxtValue.toUpperCase().indexOf(inputtedValue) > -1) {
            tr[i].style.display = "";
        } else {
            tr[i].style.display = "none";
        }
    }

});

document.getElementById("inputSearchPAT").addEventListener("keyup", (e) => {

    let inputtedValue = e.target.value.toUpperCase();

    var table = document.getElementById("AfPatBody");
    var tr = table.getElementsByTagName("tr");

    for (var i = 0; i < tr.length; i++) {

        var chapaTd = tr[i].getElementsByClassName("patcell")[0];
        /*var nomeTd = tr[i].getElementsByClassName("nomeemployee")[0];*/

        var chapaTxtValue = chapaTd ? (chapaTd.textContent || chapaTd.innerText) : "";
        /*var nomeTxtValue = nomeTd ? (nomeTd.textContent || nomeTd.innerText) : "";*/

        if (chapaTxtValue.toUpperCase().indexOf(inputtedValue) > -1) {
            tr[i].style.display = "";
        } else {
            tr[i].style.display = "none";
        }
    }

});


const loaderButtons = (message) => {

    //document.querySelectorAll('.itemCheckbox').forEach(checkbox => {
    //    checkbox.style.pointerEvents = 'none';
    //    checkbox.style.opacity = '0.6';
    //});


    document.querySelectorAll('.btnCancel').forEach(CancelBtn => {
        CancelBtn.disabled = true;
    });

    document.querySelectorAll('.btnTransfer').forEach(TransferBtn => {
        TransferBtn.disabled = true;
    });

    document.getElementById('GoBackToIndex').disabled = true;

    document.getElementById('finalRfid').readOnly = true;

    document.getElementById('loadingDivision').classList.remove('visually-hidden');

    document.getElementById('spinnerDivName').innerText = message;


    return () => {

        document.querySelectorAll('.btnCancel').forEach(CancelBtn => {
            CancelBtn.disabled = false;
        });

        document.querySelectorAll('.btnTransfer').forEach(TransferBtn => {
            TransferBtn.disabled = false;
        });

        document.getElementById('GoBackToIndex').disabled = false;

        document.getElementById('finalRfid').readOnly = false;

        document.getElementById('loadingDivision').classList.add('visually-hidden');

        document.getElementById('spinnerDivName').innerText = '';

    }


}


