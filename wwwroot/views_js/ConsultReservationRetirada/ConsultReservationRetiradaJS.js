
function ddlCatalogoChangedGestor(selectElement) {
    var selectedValue = selectElement.value;
    console.log("Selected value:", selectedValue);

    $("#ddlClasse").empty();
    $("#ddlTipo").empty();

    if (selectedValue !== "") {
        $.ajax({
            type: "GET",
            url: "/Gestor/LoadClasse",
            data: { selectedValue: selectedValue },
            success: function (data) {
                //$("#ddlClasse").empty();
                $("#ddlClasse").empty().append($('<option>', {
                    value: '', // Set an empty value for the default option
                    text: 'Selecionar...'
                }));
                // Populate the second dropdown with the fetched data
                $.each(data, function (index, item) {
                    $("#ddlClasse").append($('<option>', {
                        value: item.id, // Use the IdCategoria property
                        text: item.nome // Use the Nome property
                        //css: { color: 'red' }

                    }));
                });
            }
        });
    }


}

function ddlClasseChangedGestor(selectElement) {
    var selectedValue = selectElement.value;
    console.log("Selected value:", selectedValue);

    if (selectedValue !== "") {
        $.ajax({
            type: "GET",
            url: "/Gestor/LoadTipo",
            data: { selectedValue: selectedValue },
            success: function (data) {
                $("#ddlTipo").empty().append($('<option>', {
                    value: '', // Set an empty value for the default option
                    text: 'Selecionar...'
                }));
                // Populate the second dropdown with the fetched data
                $.each(data, function (index, item) {
                    $("#ddlTipo").append($('<option>', {
                        value: item.id, // Use the IdCategoria property
                        text: item.nome // Use the Nome property
                        //css: { color: 'red' }

                    }));
                });
            }
        });
    }

}



document.addEventListener('DOMContentLoaded', function () {

    /*console.log('working dom');*/



    const bsCollapse = new bootstrap.Collapse('#collapseExample', {
        toggle: true
    });



    //var myCollapse = new bootstrap.Collapse(document.getElementById('collapseExample'), {
    //    toggle: false, // don't toggle immediately
    //    show: true     // show it initially
    //});
});


document.getElementById('consultationForm').addEventListener('submit',async function (event) {
    event.preventDefault();

    let cleanup;

    try {

        cleanup = loaderChapaFilter();

        let orderno = document.getElementById('inputFilter').value;
        if (!orderno) throw new ValidationError('Por favor insira No. Pedido');

        const url = `/ConsultReservationRetirada/GetOrderInformation?OrderNo=${orderno}`;

        const result = await fetchJson(url);

        if (!result) throw new Error(`Nenhum resultado encontrado com ControlNo:${OrderNo}`);

        popuateInformation(result);

        if (cleanup) cleanup();

    } catch (error) {

        if (cleanup) cleanup();
        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else if (error instanceof ValidationError) {
            appendAlert(error.message, 'warning');
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

    }


    //let orderno = document.getElementById('inputFilter').value;

    //if (orderno !== "") {

    //    $.ajax({
    //        url: '/ConsultReservationRetirada/GetOrderInformation',
    //        type: 'GET', 
    //        data: { OrderNo: Number(orderno) },
    //        success: function (data) {
    //            if (data) {

    //                console.log('result', data);
    //                popuateInformation(data);

    //            } else {
    //                console.log('empty');
    //                appendAlert(`Order No:${orderno} doesnt exist.`, 'danger');
    //            }
    //        },
    //        error: function (error) {
    //            console.error('Error fetching items:', error);
    //            appendAlertWithoutAnimation(error, 'danger');
    //        }
    //    });

    //}
    //else {
    //    appendAlertWithoutAnimation('Please Input Order No', 'warning');
    //}

});


const popuateInformation = (reservationControl) => {

    document.getElementById('labelOrderNo').innerText = reservationControl.controlId;
    document.getElementById('labelType').innerText = reservationControl.controlType;
    document.getElementById('labelDateRegistration').innerText = reservationControl.dateRegistration;
    document.getElementById('labelDateExpiration').innerText = reservationControl.dateExpiration;
    document.getElementById('labelLeader').innerText = reservationControl.leaderName;


    let dateTest = MyUtils.helpers.ConvertToDateHelp(reservationControl.dateExpiration);
    console.log('date test sample', dateTest)

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (dateTest < today) {
        document.getElementById('labelDateExpiration').style.color = 'tomato';
        document.getElementById('labelDateExpiration').style.fontWeight = 'bold';
    }

    const tbody = document.getElementById('ConsultBody');
    tbody.innerHTML = '';

    reservationControl.reservationList.forEach(item => {

        const row = document.createElement('tr');

        const classeCell = document.createElement('td');
        classeCell.className = 'text-center align-middle';
        classeCell.textContent = item.classe;

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = item.itemNome;

        const memberCell = document.createElement('td');
        memberCell.className = 'text-center align-middle';
        memberCell.textContent = item.requester;

        const qtyCell = document.createElement('td');
        qtyCell.className = 'text-center align-middle';
        qtyCell.textContent = item.quantidade;


        const locationCell = document.createElement('td');
        locationCell.className = 'text-center align-middle';
        locationCell.textContent = item.ferramentaria;

        const statusCell = document.createElement('td');
        statusCell.className = 'text-center align-middle';
        statusCell.textContent = item.statusString;

        if (item.statusString === "Expired" || item.statusString === "Cancellado") {
            /*row.style.backgroundColor = "tomato";*/
            /*row.classList.add()*/
            row.classList.add("bg-danger", "bg-opacity-50");
        } else if (item.statusString === "Concluded") {
            /*row.style.backgroundColor = "green";*/
            row.classList.add("bg-success", "bg-opacity-50");
        }



        row.appendChild(classeCell);
        row.appendChild(codigoCell);
        row.appendChild(nomeCell);
        row.appendChild(memberCell);
        row.appendChild(qtyCell);
        row.appendChild(locationCell);
        row.appendChild(statusCell);
        tbody.appendChild(row);

    });


};


//function popuateInformation(reservationControl) {

//    document.getElementById('labelOrderNo').innerText = reservationControl.controlId;
//    document.getElementById('labelType').innerText = reservationControl.controlType;
//    document.getElementById('labelDateRegistration').innerText = reservationControl.dateRegistration;
//    document.getElementById('labelDateExpiration').innerText = reservationControl.dateExpiration;
//    document.getElementById('labelLeader').innerText = reservationControl.leaderName;


//    const tbody = document.getElementById('ConsultBody');
//    tbody.innerHTML = '';

//    reservationControl.reservationList.forEach(item => {

//        const row = document.createElement('tr');

//        const classeCell = document.createElement('td');
//        classeCell.className = 'text-center align-middle';
//        classeCell.textContent = item.classe;

//        const codigoCell = document.createElement('td');
//        codigoCell.className = 'text-center align-middle';
//        codigoCell.textContent = item.codigo;

//        const nomeCell = document.createElement('td');
//        nomeCell.className = 'text-center align-middle';
//        nomeCell.textContent = item.itemNome;

//        const qtyCell = document.createElement('td');
//        qtyCell.className = 'text-center align-middle';
//        qtyCell.textContent = item.quantidade;


//        const locationCell = document.createElement('td');
//        locationCell.className = 'text-center align-middle';
//        locationCell.textContent = item.ferramentaria;

//        const statusCell = document.createElement('td');
//        statusCell.className = 'text-center align-middle';
//        statusCell.textContent = item.statusString;

//        row.appendChild(classeCell);
//        row.appendChild(codigoCell);
//        row.appendChild(nomeCell);
//        row.appendChild(qtyCell);
//        row.appendChild(locationCell);
//        row.appendChild(statusCell);
//        tbody.appendChild(row);

//    });




//    //const tbody = document.getElementById('ConsultBody');
//    //tbody.innerHTML = '';

//    //const row = document.createElement('tr');

//    //const orderNoCell = document.createElement('td');
//    //orderNoCell.className = 'text-center align-middle';
//    //orderNoCell.textContent = reservationControl.controlId;

//    //const leaderCell = document.createElement('td');
//    //leaderCell.className = 'text-center align-middle';
//    //leaderCell.textContent = reservationControl.leaderName;

//    //const typeCell = document.createElement('td');
//    //typeCell.className = 'text-center align-middle';
//    //typeCell.textContent = reservationControl.controlType;

//    //const viewCell = document.createElement('td');
//    //viewCell.className = 'text-center align-middle';
//    //const openReservationModal = document.createElement('a');
//    //openReservationModal.className = 'btn btn-link';
//    //openReservationModal.textContent = "View";

//    //openReservationModal.addEventListener('click', (e) => {
//    //    openModalReservation(reservationControl.reservationList, reservationControl);
//    //});
//    //viewCell.appendChild(openReservationModal);

//    //row.appendChild(orderNoCell);
//    //row.appendChild(leaderCell);
//    //row.appendChild(typeCell);
//    //row.appendChild(viewCell);
//    //tbody.appendChild(row);

//}

function openModalReservation(reservations, control) {

    const modal = new bootstrap.Modal(document.getElementById('exampleModal'));

    const modalElement = document.getElementById('exampleModal');
    modalElement.querySelector('#OrderNoHolder').textContent = "";
    modalElement.querySelector('#OrderNoHolder').textContent = control.controlId;

    const tbody = document.getElementById('ConsultReservationBody');
    tbody.innerHTML = '';

    reservations.forEach(item => {

        const row = document.createElement('tr');

        const classeCell = document.createElement('td');
        classeCell.className = 'text-center align-middle';
        classeCell.textContent = item.classe;

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = item.itemNome;

        const qtyCell = document.createElement('td');
        qtyCell.className = 'text-center align-middle';
        qtyCell.textContent = item.quantidade;


        const locationCell = document.createElement('td');
        locationCell.className = 'text-center align-middle';
        locationCell.textContent = item.ferramentaria;

        row.appendChild(classeCell);
        row.appendChild(codigoCell);
        row.appendChild(nomeCell);
        row.appendChild(qtyCell);
        row.appendChild(locationCell);
        tbody.appendChild(row);

    });

    modal.show();
}


document.getElementById('consultPorChapa').addEventListener('submit', async (event) => {
    event.preventDefault();

    let cleanup;

    try {

        cleanup = loaderChapaFilter();

        let chapaEmployee = document.getElementById('inputChapaFilter').value;
        if (!chapaEmployee) throw new ValidationError('Por favor insira Chapa');

        const inputCodigo = document.getElementById('inputCodigo').value;
        const inputItem = document.getElementById('inputItem').value;
        const ddlCatalogoValue = document.getElementById('ddlCatalogo').value;
        const ddlClasseValue = document.getElementById('ddlClasse').value;
        const ddlTipoValue = document.getElementById('ddlTipo').value;

        const allEmpty = !inputCodigo && !inputItem && !ddlCatalogoValue && !ddlClasseValue && !ddlTipoValue;

        if (allEmpty) throw new ValidationError('Por favor, insira pelo menos 1 filtro para evitar tráfego de consultas');
           
        const formData = {
            Codigo: inputCodigo ? inputCodigo : null,
            Item: inputItem ? inputItem : null,
            IdCatalogo: ddlCatalogoValue ? parseInt(ddlCatalogoValue) : null,
            IdClasse: ddlClasseValue ? parseInt(ddlClasseValue) : null,
            IdTipo: ddlTipoValue ? parseInt(ddlTipoValue) : null,
            Chapa: chapaEmployee
        };

        const queryParams = new URLSearchParams();
        Object.keys(formData).forEach(key => {
            if (formData[key] !== null && formData[key] !== undefined) {
                queryParams.append(key, formData[key]);
            }
        });

        const url = `/ConsultReservationRetirada/GetReservationEmployee?${queryParams.toString()}`;

        const result = await fetchJson(url);

        if (result.length == 0) throw new Error('Nenhum resultado encontrado.');

        populateReservationEmployee(result);

        const bsCollapse = bootstrap.Collapse.getInstance('#collapseExample');
        bsCollapse.toggle();

        if (cleanup) cleanup();

    } catch (error) {

        if (cleanup) cleanup();
        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else if (error instanceof ValidationError) {
            appendAlert(error.message, 'warning');
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

        /*return null;*/

    }



    //var isValid = true;

    //let chapaEmployee = document.getElementById('inputChapaFilter').value;

    //const searchBtn = document.getElementById('btnSubmitSearch');
    //const loadingSpan = document.getElementById('loadingSpanSearch');

    //if (chapaEmployee !== "") {

    //    const inputCodigo = document.getElementById('inputCodigo').value;
    //    const inputItem = document.getElementById('inputItem').value;
    //    const ddlCatalogoValue = document.getElementById('ddlCatalogo').value;
    //    const ddlClasseValue = document.getElementById('ddlClasse').value;
    //    const ddlTipoValue = document.getElementById('ddlTipo').value;

    //    const allEmpty = !inputCodigo && !inputItem && !ddlCatalogoValue && !ddlClasseValue && !ddlTipoValue;

    //    if (allEmpty) {
    //        isValid = false;
    //        console.log(isValid);
    //        appendAlert('Por favor, insira pelo menos 1 filtro para evitar tráfego de consultas', 'warning');
    //        return;
    //    }

    //    searchBtn.disabled = true;
    //    loadingSpan.classList.remove('visually-hidden');

    //    const formData = {
    //        Codigo: inputCodigo ? inputCodigo : null,
    //        Item: inputItem ? inputItem : null,
    //        IdCatalogo: ddlCatalogoValue ? parseInt(ddlCatalogoValue) : null,
    //        IdClasse: ddlClasseValue ? parseInt(ddlClasseValue) : null,
    //        IdTipo: ddlTipoValue ? parseInt(ddlTipoValue) : null,
    //        Chapa: chapaEmployee
    //    };

    //    try {

    //        const response = await fetch('/ConsultReservationRetirada/GetReservationEmployee', {
    //            method: 'POST',
    //            headers: {
    //                'Content-Type': 'application/json'
    //            },
    //            body: JSON.stringify(formData)
    //        })

    //        if (!response.ok) {
    //            throw new Error(`HTTP error! Status: ${response.status}`);
    //        }

    //        const data = await response.json();

    //        console.log(data);

    //        if (data.success) {
    //            console.log(data.listReservation);
    //            populateReservationEmployee(data.listReservation);

    //            /*bootstrap.Modal.getInstance(document.getElementById('transferModal'))*/
    //            const bsCollapse = bootstrap.Collapse.getInstance('#collapseExample');
    //            bsCollapse.toggle();
    //            console.log(bsCollapse);
    //            searchBtn.disabled = false;
    //            loadingSpan.classList.add('visually-hidden');

    //        } else {
    //            appendAlert(data.message, 'danger');
    //            searchBtn.disabled = false;
    //            loadingSpan.classList.add('visually-hidden');
    //        }

    //    } catch (error) {
    //        console.error('Error fetching items:', error);
    //        appendAlertWithoutAnimation(error, 'danger');
    //        throw error;
    //    }

    //}
    //else {
    //    isValid = false;
    //    appendAlert('Por favor insira Chapa', 'warning');
    //    return;
    //}

});


const loaderChapaFilter = () => {

    document.getElementById('btnOrderNo').disabled = true;
    document.getElementById('loadingSpanSearchOrder').classList.remove('visually-hidden');
    document.getElementById('btnSubmitSearch').disabled = true;
    document.getElementById('btnLimpar').disabled = true;
    document.getElementById('loadingSpanSearch').classList.remove('visually-hidden');

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
    });

    document.querySelectorAll('.form-select').forEach(select => {
        select.style.pointerEvents = 'none';
        select.style.backgroundColor = '#f8f9fa';
    });

    document.querySelectorAll('.nav-link').forEach(link => {
        link.disabled = true;
    });


    return () => {
        document.getElementById('btnOrderNo').disabled = false;
        document.getElementById('loadingSpanSearchOrder').classList.add('visually-hidden');
        document.getElementById('btnSubmitSearch').disabled = false;
        document.getElementById('btnLimpar').disabled = false;
        document.getElementById('loadingSpanSearch').classList.add('visually-hidden');

        document.querySelectorAll('.form-control').forEach(input => {
            input.readOnly = false;
        });

        document.querySelectorAll('.form-select').forEach(select => {
            select.style.pointerEvents = '';
            select.style.backgroundColor = '';
        });

        document.querySelectorAll('.nav-link').forEach(link => {
            link.disabled = false;
        });
    }
}


const populateReservationEmployee = (reservationsResult) => {

    const tbody = document.getElementById('consultEmployeeBody');
    tbody.innerHTML = '';

    reservationsResult.forEach(item => {

        const row = document.createElement('tr');

        const classeCell = document.createElement('td');
        classeCell.className = 'text-center align-middle';
        classeCell.textContent = item.classe;

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = item.itemNome;

        const memberCell = document.createElement('td');
        memberCell.className = 'text-center align-middle';
        memberCell.textContent = item.memberInfo.chapa;

        const leaderCell = document.createElement('td');
        leaderCell.className = 'text-center align-middle';
        leaderCell.textContent = item.leaderInfo.chapa;

        const qtdCell = document.createElement('td');
        qtdCell.className = 'text-center align-middle';
        qtdCell.textContent = item.quantidade;

        const locationCell = document.createElement('td');
        locationCell.className = 'text-center align-middle';
        locationCell.textContent = item.ferramentaria;

        const orderCell = document.createElement('td');
        orderCell.className = 'text-center align-middle';
        orderCell.textContent = item.orderNo;

        const tipoCell = document.createElement('td');
        tipoCell.className = 'text-center align-middle';
        tipoCell.textContent = item.reservationType;

        const statusCell = document.createElement('td');
        statusCell.className = 'text-center align-middle';
        statusCell.textContent = item.statusString;

        row.appendChild(classeCell);
        row.appendChild(codigoCell);
        row.appendChild(nomeCell);
        row.appendChild(memberCell);
        row.appendChild(leaderCell);
        row.appendChild(qtdCell);
        row.appendChild(locationCell);
        row.appendChild(orderCell);
        row.appendChild(tipoCell);
        row.appendChild(statusCell);
        tbody.appendChild(row);

    });

}

