


if (passeddata.length > 0) {
    /*console.log(passeddata);*/
    makeReservationTableList(passeddata);
}

function makeReservationTableList(Products) {

    const tbody = document.getElementById("ReservationConfirmationBody");
    tbody.innerHTML = '';

    Products.forEach((item, index) => {

        /*console.log("index",index);*/

        const row = document.createElement('tr');
        row.id = `row-${item.idReservation}`;
        row.dataset.reservationId = item.idReservation; 

        const orderNoCell = document.createElement('td');
        orderNoCell.className = 'text-center align-middle';
        orderNoCell.textContent = item.idReservationControl;

        orderNoCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].IdReservationControl`, item.idReservationControl));
        orderNoCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].IdReservation`, item.idReservation));
        orderNoCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].IdProduto`, item.idProduto));

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const itemCell = document.createElement('td');
        itemCell.className = 'text-center align-middle';
        itemCell.textContent = item.itemNome;

        const stockCell = document.createElement('td');
        stockCell.className = 'text-center align-middle';
        stockCell.textContent = item.qtyStock;

        const qtyReqCell = document.createElement('td');
        qtyReqCell.className = 'text-center align-middle';
        const input = document.createElement('input');
        input.type = 'number';
        input.className = 'form-control digitsonly';
        input.min = '1'; 
        input.step = '1';
        input.max = item.qtyRequested; 
        input.name = `FinalProcessList[${index}].QtyRequested`;
        input.value = item.qtyRequested;

        input.addEventListener('change', (e) => {

            let inputValue = Number(e.target.value);

            if (!inputValue) {
                /*appendAlert(`The inputted QtyRequested for cannot be 0 or empty ${item.codigo}`, 'warning');  */
                appendAlert(`A Qty.Solicitada inserida para ${item.codigo} não pode ser 0 ou vazia`, 'warning');  
                e.target.value = 1;
                return;
            }

            if (inputValue > Number(item.qtyRequested)) {
                appendAlert(`A Qtd.Solicitada inserida não pode ser maior que a Solicitada para ${item.codigo}`, 'warning');
                e.target.value = 1;
                return;
            }

            //if (inputValue > Number(item.qtyStock)) {
            //    appendAlert(`The inputted QtyRequested for cannot be greater than the Stock Quantity for ${item.codigo}`, 'warning');
            //    e.target.value = 1;
            //    return;
            //}

        });

        qtyReqCell.appendChild(input);
        /*qtyReqCell.textContent = item.qtyRequested;*/

        const retornoCell = document.createElement('td');
        retornoCell.className = 'text-center align-middle';
        const inputRetorno = document.createElement('input');
        inputRetorno.type = 'date';
        inputRetorno.className = 'form-control';
        inputRetorno.name = `FinalProcessList[${index}].DateReturn`;
        inputRetorno.addEventListener('change', (e) => {
            const selectedDate = new Date(e.target.value);
            const today = new Date();

            // Set time of both to midnight to compare only date part
            selectedDate.setHours(0, 0, 0, 0);
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                appendAlert(`A data selecionada não pode ser anterior a hoje para ${item.codigo}.`, 'warning');
                e.target.value = ''; // Optional: reset the invalid date
            }
        });
        retornoCell.appendChild(inputRetorno);

        const solicitanteCell = document.createElement('td');
        solicitanteCell.className = 'text-center align-middle';

        const solicitanteLink = document.createElement("a");
        solicitanteLink.setAttribute("href", "#");
        solicitanteLink.textContent = item.memberInfo.chapa;
        solicitanteLink.onclick = function (e) {
            e.preventDefault();

            openEmployeeInformation(item.memberInfo, 'Solicitante');
        };
        solicitanteCell.appendChild(solicitanteLink);

        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].IdTerceiroSolicitante`, Number(0)));
        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].ChapaSolicitante`, item.memberInfo.chapa));
        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].CodColigadaSolicitante`, item.memberInfo.codColigada));


        const liberadorCell = document.createElement('td');
        liberadorCell.className = 'text-center align-middle';

        const liberadorLink = document.createElement("a");
        liberadorLink.setAttribute("href", "#");
        liberadorLink.textContent = item.leaderInfo.chapa;
        liberadorLink.onclick = function (e) {
            e.preventDefault();

            openEmployeeInformation(item.leaderInfo, 'Liberador');
        };
        liberadorCell.appendChild(liberadorLink);
        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].IdTerceiroLiberador`, Number(0)));
        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].ChapaLiberador`, item.leaderInfo.chapa));
        solicitanteCell.appendChild(HiddenInputProcess(`FinalProcessList[${index}].CodColigadaLiberador`, item.leaderInfo.codColigada));

        const obsCell = document.createElement('td');
        obsCell.className = 'text-center align-middle';
        const inputObs = document.createElement('input');
        inputObs.className = 'form-control digitsonly';
        inputObs.name = `FinalProcessList[${index}].Observacao`;
        obsCell.appendChild(inputObs);


        const cancelCell = document.createElement('td');
        cancelCell.className = 'text-center align-middle';

        const cancelButton = document.createElement('button');
        cancelButton.className = 'btn btn-danger cancelBtn';
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
            transferButton.className = 'btn btn-warning ms-1 text-white';
            transferButton.type = 'button';
            transferButton.textContent = 'Transferir';

            transferButton.addEventListener('click', async function () {

                /*getTransferList(item.idCatalogo, item.idFerramentaria, row);*/

                const listFerramentaria = await getTransferList(item.idCatalogo, item.idFerramentaria);
                if (!listFerramentaria || listFerramentaria.length === 0) {
                    appendAlert('Não há ferramentaria disponível para transferência', 'danger');
                    return;
                }

                openTransferModal(row, listFerramentaria);

                /*console.log(listFerramentaria);*/

                /*openTransferModal(row);*/
            });

            cancelCell.appendChild(transferButton);
        }
 

        row.appendChild(orderNoCell);
        row.appendChild(codigoCell);
        row.appendChild(itemCell);
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

    modalElement.querySelector('#chapaLabel').textContent = "";
    modalElement.querySelector('#chapaLabel').textContent = employee.chapa;

    modalElement.querySelector('#nomeLabel').textContent = "";
    modalElement.querySelector('#nomeLabel').textContent = employee.nome;

    modalElement.querySelector('#situacaoLabel').textContent = "";
    modalElement.querySelector('#situacaoLabel').textContent = employee.codSituacao;

    modalElement.querySelector('#funcaoLabel').textContent = "";
    modalElement.querySelector('#funcaoLabel').textContent = employee.funcao;

    modalElement.querySelector('#secaoLabel').textContent = "";
    modalElement.querySelector('#secaoLabel').textContent = employee.secao;

    modal.show();
}


const openTransferModal = (row, listFerramentaria) => {

    const modal = new bootstrap.Modal(document.getElementById('transferModal'));
    const modalElement = document.getElementById('transferModal');

    const tableBody = modalElement.querySelector("#reservationTransferBody");
    tableBody.innerHTML = '';

    const transferRow = document.createElement('tr');
    transferRow.id = row.id;
    transferRow.dataset.reservationId = row.dataset.reservationId; 

    const orderNoCell = document.createElement('td');
    orderNoCell.className = 'text-center align-middle';
    orderNoCell.textContent = row.cells[0].textContent.trim();

    const codigoCell = document.createElement('td');
    codigoCell.className = 'text-center align-middle';
    codigoCell.textContent = row.cells[1].textContent.trim();

    const nameCell = document.createElement('td');
    nameCell.className = 'text-center align-middle';
    nameCell.textContent = row.cells[2].textContent.trim();

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

        const url = `/PrepareReservation/TransferReservation`;

        const formData = new URLSearchParams();
        formData.append('OrderNo', orderNo);
        formData.append('IdFerramentaria', IdFerramentaria);
        formData.append('Observacao', Observacao);

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

        const matchingRow = document.querySelector(`#ReservationConfirmationBody tr[data-reservation-id="${orderNo}"]`);
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

    //const response = await fetch('/PrepareReservation/TransferReservation', {
    //    method: 'POST',
    //    body: formData,
    //});

    //if (!response.ok) {
    //    throw new Error(`HTTP error! Status: ${response.status}`);
    //}

    //const data = await response.json();

    //if (data.success) {

    //    const matchingRow = document.querySelector(`#ReservationConfirmationBody tr[data-reservation-id="${orderNo}"]`);
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

    return () => {
        modalElement.removeEventListener('hide.bs.modal', preventHide);
        closeButtons.forEach(btn => btn.style.pointerEvents = 'auto');
        btn.disabled = false;
        btn.querySelector('#loadingConfirmation').classList.add('visually-hidden');
    };

}





const getTransferList = async (idCatalogo, idFerramentaria) => {

    try {

        showTransactionLoader('Obtendo ferramentaria disponível...');

        const url = `/PrepareReservation/GetFerramentariaTransferList?IdCatalogo=${idCatalogo}&IdFerramentaria=${idFerramentaria}`;

        const ferramentariaList = await fetchJson(url);

        hideTransactionLoader();
        return ferramentariaList;

    } catch (error) {

        hideTransactionLoader();

        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

        return null;

    }

    //try {

        

    //    const url = `/PrepareReservation/GetFerramentariaList?IdCatalogo=${idCatalogo}&IdFerramentaria=${idFerramentaria}`;

    //    const response = await fetchJson(url);

    //    const response = await fetch(url, { method: 'GET' });

    //    if (!response.ok) {
    //        throw new Error(`HTTP error! Status: ${response.status}`);
    //    }

    //    const data = await response.json();

    //    if (data.success) {
    //        return data.listFerramentaria;
    //    } else {
    //        appendAlert(data.message, 'danger');
    //    }


    //} catch (error) {
    //    console.error('Error fetching items:', error);
    //    appendAlertWithoutAnimation(error, 'danger');
    //    throw error; // Re-throw to handle in the calling function
    //}


    //$.ajax({
    //    url: '/PrepareReservation/GetFerramentariaList',
    //    type: 'GET',
    //    data: { IdCatalogo: idCatalogo, IdFerramentaria: idFerramentaria },
    //    success: function (data) {
    //        if (data.success) {

    //            callback(data.listFerramentaria); 
    //            /*openTransferModal(row, data.listFerramentaria);*/

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


async function cancelTransaction(row, IdReservation, cancelObservacao) {

    //console.log('row', row);
    //console.log('IdReservation', IdReservation);

    showTransactionLoader(`Item: ${row.cells[1].textContent} - Cancelando...`);

    try {

        const url = `/PrepareReservation/CancelTransaction`;

        //const dataToSend = {
        //    IdReservation: IdReservation,
        //    cancelObservacao: cancelObservacao
        //};

        const formData = new URLSearchParams();
        formData.append('IdReservation', IdReservation);
        formData.append('cancelObservacao', cancelObservacao);

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
        hideTransactionLoader();

    } catch (error) {

        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

        hideTransactionLoader();

    }
    

    //$.ajax({
    //    url: '/PrepareReservation/CancelTransaction',
    //    type: 'POST',
    //    data: { IdReservation: IdReservation, cancelObservacao: cancelObservacao },
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

const showTransactionLoader = (message) => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });

    document.getElementById('spinnerDiv').classList.add('text-danger');
    document.getElementById('loadingDivision').classList.remove('visually-hidden');
    document.getElementById('spinnerDivName').innerText = message;

}

const hideTransactionLoader = () => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = false;
    });

    document.getElementById('spinnerDiv').classList.remove('text-danger');
    document.getElementById('loadingDivision').classList.add('visually-hidden');
    document.getElementById('spinnerDivName').innerText = '';

}



function goBackToIndex() {
    /*window.location.href = '@Url.Action("Index", "CLVM")';*/

    window.location.href = `/PrepareReservation/Index`;
}


function HiddenInputProcess(name, value) {
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = name;
    input.value = value;
    return input;
}


document.getElementById('formFinalReservation').addEventListener("submit", async (e) => {
    e.preventDefault();
   
    const rows = document.querySelectorAll('#ReservationConfirmationBody tr');
    let isValid = true;
    const errors = [];

    let cleanup;

    cleanup = finalizeLoader();

    rows.forEach((row, index) => {

        const productCode = row.cells[1].textContent;

        console.log(productCode);

        const qtyInput = row.querySelector('input[name*="QtyRequested"]');
        const maxQty = parseInt(qtyInput.max);
        const qtyValue = parseInt(qtyInput.value);

        if (qtyValue < 1) {
            isValid = false;
            errors.push(`Quantity must be at least 1 for ${productCode}`);
        } else if (qtyValue > maxQty) {
            isValid = false;
            errors.push(`Quantity cannot exceed ${maxQty} for ${productCode}`);
        }

        const idProdutoInput = row.querySelector('input[name*="IdProduto"]');
        if (!idProdutoInput.value) {
            isValid = false;
            errors.push(`No IdProduto is selected for ${productCode}`);
        }

        const idReservationInput = row.querySelector('input[name*="IdReservation"]');
        if (!idReservationInput.value) {
            isValid = false;
            errors.push(`No IdReservation for ${productCode}`);
        }

        const dateInput = row.querySelector('input[name*="DateReturn"]');
        if (dateInput.value) {
            const selectedDate = new Date(dateInput.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                isValid = false;
                errors.push(`Return date cannot be in the past for ${productCode}`);
            }
        }

    });

    console.log(e.target);

    if (isValid) {

        try {

            /*const url = `/PrepareReservation/CancelTransaction`;*/

            const formData = new FormData(e.target);

            const result = await fetchJson(e.target.action, {
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

            /*if (cleanup) cleanup();*/

            appendAlert(result.message, 'success');

            setTimeout(function () {
                window.location.href = `/PrepareReservation/Index`;
            }, 2000);

        }
        catch (error) {

            if (cleanup) cleanup();
            console.error(error);
            if (error.status === 401 || error.status === 403) {
                handleAuthenticationError(error);
            } else {
                appendAlertWithoutAnimation(error.message, 'danger');
            }

            hideTransactionLoader();

        }

    } else {
        if (cleanup) cleanup();
        const message = errors.join('<br>');
        appendAlertWithoutAnimation(message, 'danger');
        e.preventDefault();
    }

});

const finalizeLoader = () => {

    document.getElementById('ProcessSubmitBtn').disabled = true;
    document.getElementById('loadingFull').classList.remove('visually-hidden');

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });


    return () => {
        document.getElementById('ProcessSubmitBtn').disabled = false;
        document.getElementById('loadingFull').classList.add('visually-hidden');
        document.querySelectorAll('.btn').forEach(btn => {
            btn.disabled = false;
        });
    };
}

function reindexFormRows() {
    const rows = document.querySelectorAll('#ReservationConfirmationBody tr');

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