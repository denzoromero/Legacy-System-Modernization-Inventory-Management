//console.log('working');


//console.log(ReservationList);

document.addEventListener('DOMContentLoaded', function () {

    let inputElement = document.getElementById('finalRfid');
    inputElement.value = '';

    //inputElement.addEventListener('copy', e => e.preventDefault());
    //inputElement.addEventListener('cut', e => e.preventDefault());
    //inputElement.addEventListener('paste', e => e.preventDefault());

});


if (ReservationList.length > 0) {
    /*console.log('Reservation is not empty');*/

    makeTableReservation(ReservationList);
}

function makeTableReservation(Items) {

    const tbody = document.getElementById('ReservationBody');
    tbody.innerHTML = '';

    Items.forEach((item, index) => {

        const row = document.createElement('tr');
        row.setAttribute("data-index", index);

        const orderNoCell = document.createElement('td');
        orderNoCell.className = 'text-center align-middle';
        orderNoCell.textContent = item.idReservationControl;
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdReservationControl`, item.idReservationControl));
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdReservation`, item.idReservation));
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdProductReservation`, item.idProductReservation));
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdProduto`, item.idProduto));
        orderNoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].IdObra`, item.idObra));


        const classeCell = document.createElement('td');
        classeCell.className = 'text-center align-middle';
        classeCell.textContent = item.classe;

        const typeCell = document.createElement('td');
        typeCell.className = 'text-center align-middle';
        typeCell.textContent = item.type;

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = item.nome;

        const qtyReqCell = document.createElement('td');
        qtyReqCell.className = 'text-center align-middle';
        qtyReqCell.textContent = item.qtyFinal;
        qtyReqCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].QtyRequested`, item.qtyFinal));

        const dataRetornoCell = document.createElement('td');
        dataRetornoCell.className = 'text-center align-middle';
        dataRetornoCell.textContent = item.dateReturn;
        dataRetornoCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].DateReturn`, item.dateReturnProper));

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
        obsCell.textContent = item.observacao;
        obsCell.appendChild(MyUtils.helpers.MakeHiddenInputUni(`FinalProcessList[${index}].Observacao`, item.observacao));

        const checkCell = document.createElement('td');
        checkCell.className = 'text-center align-middle';

        const checkBoxAll = document.createElement('input');
        checkBoxAll.className = 'form-check-input itemCheckbox';
        checkBoxAll.type = 'checkbox';
        checkBoxAll.value = true;
        checkBoxAll.checked = true;
        checkBoxAll.name = `FinalProcessList[${index}].IsSelected`;
        checkCell.appendChild(checkBoxAll);
    

        row.appendChild(orderNoCell);
        row.appendChild(classeCell);
        row.appendChild(typeCell);
        row.appendChild(codigoCell);
        row.appendChild(nomeCell);
        row.appendChild(qtyReqCell);
        row.appendChild(dataRetornoCell);
        row.appendChild(solicitanteCell);
        row.appendChild(liberadorCell);
        row.appendChild(obsCell);
        row.appendChild(checkCell);
        tbody.appendChild(row);
    });
}


function openEmployeeInformation(employee, titleName) {

    /*console.log(employee);*/

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
    modalElement.querySelector('#situacaoLabel').textContent = employee.codSituacao;
    modalElement.querySelector('#funcaoLabel').textContent = employee.funcao;
    modalElement.querySelector('#secaoLabel').textContent = employee.secao;


    modal.show();
}

//document.getElementById("finalRfid").addEventListener("change", function (event) {

//    event.preventDefault();

//    /*console.log('trigger change finalrfid')*/

//    //var form = document.getElementById('FormFinalizeReservation');

//    //form.submit();

//    if (event.target.value !== "") {
//        /*formSubmissionAction();*/
//        console.log('trigger change finalrfid');
//    } else {
//        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
//    }

//});

let rfidTimeout;

document.getElementById('finalRfid').addEventListener('input', async (event) => {

    let cleanup 

    clearTimeout(rfidTimeout);

    rfidTimeout = setTimeout(async () => {

        try {

            const errors = [];
            let isValid = true;

            cleanup = loaderTransaction(event);

            if (!event.target.value.trim()) {
                throw new Error('Por favor, insira o número do crachá.');
            }

            const allCheckboxes = document.querySelectorAll('#ReservationBody .itemCheckbox');
            const allUnselected = Array.from(allCheckboxes).every(cb => !cb.checked);
            if (allUnselected) {
                throw new Error('Por favor, selecione pelo menos 1 item para continuar.');
            }

            const rows = document.querySelectorAll('#ReservationBody tr');
            rows.forEach((row, index) => {

                const checkBox = row.querySelector(`input[type="checkbox"][name="FinalProcessList[${index}].IsSelected"]`);
                const isChecked = checkBox ? checkBox.checked : false;

                const codigo = row.cells[3].textContent;

                if (isChecked) {

                    const input = row.querySelector(`input[type="hidden"][name="FinalProcessList[${index}].IdProduto"]`);
                    if (!input || !input.value) {
                        row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                        errors.push(`${codigo} doesnt have IdProduto Selected`);
                    }

                    const inputQty = row.querySelector(`input[type="hidden"][name="FinalProcessList[${index}].QtyRequested"]`);
                    if (!inputQty || !inputQty.value) {
                        row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                        errors.push(`${codigo} doesnt have Qty Inputted`);
                    }

                }

            });

            if (errors.length > 0) {
                throw new Error(errors.join('<br>'));
            }

            var form = document.getElementById('FormFinalizeReservation');

            const formData = new FormData(form);

            const result = await fetchJson(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    /*'Content-Type': 'multipart/form-data',*/
                    'Accept': 'application/json'
                }
            });

            console.log(result);


            if (!result.success) {
                throw new Error(result.message);
            }


            appendAlert(result.message, 'success');

            setTimeout(function () {
                window.location.href = `/HandoutReservation/Index`;
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




});

window.addEventListener('beforeunload', () => {
    clearTimeout(rfidTimeout);
});


const loaderTransaction = (event) => {

    const input = event.target;

    document.getElementById('loadingConfirmation').classList.remove('visually-hidden');
    input.readOnly = true;

    document.getElementById('GoBackToIndex').disabled = true;

    //document.querySelectorAll('.itemCheckbox').forEach(checkbox => {
    //    checkbox.readOnly = true;
    //});

    document.querySelectorAll('.itemCheckbox').forEach(checkbox => {
        checkbox.style.pointerEvents = 'none';
        checkbox.style.opacity = '0.6';
    });


    return () => {
        document.getElementById('loadingConfirmation').classList.add('visually-hidden');
        input.readOnly = false;
        document.getElementById('GoBackToIndex').disabled = false;
        input.value = '';
        //document.querySelectorAll('.itemCheckbox').forEach(checkbox => {
        //    checkbox.readOnly = false;
        //});
        document.querySelectorAll('.itemCheckbox').forEach(checkbox => {
            checkbox.style.pointerEvents = '';
            checkbox.style.opacity = '';
        });
    }

}



document.getElementById('FormFinalizeReservation').addEventListener('submit', function (event) {

    event.preventDefault();

    let inputCracha = document.getElementById('finalRfid').value;

    if (inputCracha !== "") {
            formSubmissionAction();
            console.log('Form submitted!');
    } else {
            appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
    }

});

function formSubmissionAction() {

    var form = document.getElementById('FormFinalizeReservation');

    const inputCracha = document.getElementById('finalRfid');

    var isValid = true;

    var crachaInput = inputCracha.value;
    console.log(crachaInput);
    if (!crachaInput) {
        isValid = false;
        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
        return;
    }

    const tbody = document.getElementById('ReservationBody');
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

            const inputQty = row.querySelector(`input[type="hidden"][name="FinalProcessList[${sample}].QtyRequested"]`);
            if (!inputQty || !inputQty.value) {
                console.log('no Qty inputted');
                appendAlertWithoutAnimation(`${codigo} doesnt have Qty Inputted`, 'warning');
                row.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
                isValid = false;
                inputCracha.value = "";
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

    window.location.href = `/HandoutReservation/Index`;
}