
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


document.getElementById('formGestorSearch').addEventListener('submit', async function (event) {
    event.preventDefault(); 

    let cleanup;

    try {

        cleanup = loadingGestor();

        const inputCodigo = document.getElementById('inputCodigo').value;
        const inputItem = document.getElementById('inputItem').value;
        const ddlCatalogoValue = document.getElementById('ddlCatalogo').value;
        const ddlClasseValue = document.getElementById('ddlClasse').value;
        const ddlTipoValue = document.getElementById('ddlTipo').value;
        const checkValue = document.getElementById('checkReservadoRetirada').checked;

        const allEmpty = !inputCodigo && !inputItem && !ddlCatalogoValue && !ddlClasseValue && !ddlTipoValue;

        if (allEmpty) throw new ValidationError('Por favor, insira pelo menos 1 filtro para evitar tráfego de consulta');

        const formData = {
            Codigo: inputCodigo ? inputCodigo : null,
            Item: inputItem ? inputItem : null,
            IdCatalogo: ddlCatalogoValue ? parseInt(ddlCatalogoValue) : null,
            IdClasse: ddlClasseValue ? parseInt(ddlClasseValue) : null,
            IdTipo: ddlTipoValue ? parseInt(ddlTipoValue) : null,
            IsChecked: checkValue
        };

        const queryParams = new URLSearchParams();
        Object.keys(formData).forEach(key => {
            if (formData[key] !== null && formData[key] !== undefined) {
                queryParams.append(key, formData[key]);
            }
        });

        const url = `/GestorRetiradaReserva/GetGestorReservationRetirada?${queryParams.toString()}`;

        const result = await fetchJson(url);

        if (result.length == 0) throw new Error('Nenhum resultado encontrado.');

        populateGestorTable(result);

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


    //var isValid = true;

    //const inputCodigo = document.getElementById('inputCodigo').value;
    //const inputItem = document.getElementById('inputItem').value;
    //const ddlCatalogoValue = document.getElementById('ddlCatalogo').value;
    //const ddlClasseValue = document.getElementById('ddlClasse').value;
    //const ddlTipoValue = document.getElementById('ddlTipo').value;
    //const checkValue = document.getElementById('checkReservadoRetirada').checked;

    //console.log(ddlCatalogoValue);
    //console.log(ddlClasseValue);
    //console.log(ddlTipoValue);
    //console.log(checkValue);

    //const allEmpty = !inputCodigo && !inputItem && !ddlCatalogoValue && !ddlClasseValue && !ddlTipoValue;


    //if (allEmpty) {
    //    isValid = false;
    //    console.log(isValid);
    //    appendAlertWithoutAnimation('Por favor, insira pelo menos 1 filtro para evitar tráfego de consulta', 'warning');
    //    return;
    //}

    //const SearchButton = document.getElementById('btnSearch');
    //const loadingSpan = document.getElementById('loadingSpan');

    //if (isValid) {

    //    const formData = {
    //        Codigo: inputCodigo ? inputCodigo : null,
    //        Item: inputItem ? inputItem : null,
    //        IdCatalogo: ddlCatalogoValue ? parseInt(ddlCatalogoValue) : null,
    //        IdClasse: ddlClasseValue ? parseInt(ddlClasseValue) : null,
    //        IdTipo: ddlTipoValue ? parseInt(ddlTipoValue) : null,
    //        IsChecked: checkValue
    //    };

    //    SearchButton.disabled = true;
    //    loadingSpan.classList.remove('visually-hidden');


    //    fetch('/GestorRetiradaReserva/GetGestorReservationRetirada', {
    //        method: 'POST',
    //        headers: {
    //            'Content-Type': 'application/json'
    //        },
    //        body: JSON.stringify(formData)
    //    })
    //        .then(response => {
    //            if (!response.ok) throw new Error("Network response was not ok");
    //            return response.json();
    //        })
    //        .then(data => {
    //            console.log('AJAX response:', data);
    //            // Update your UI with the data

    //            if (data.length > 0) {

    //                SearchButton.disabled = false;
    //                loadingSpan.classList.add('visually-hidden');

    //                const tbody = document.getElementById('gestorRRBody');
    //                tbody.innerHTML = '';

    //                data.forEach((product) => {

    //                    product.ferramentarias.forEach((ferramentaria) => {

    //                        const row = document.createElement('tr');

    //                        const ferramentariaCell = document.createElement('td');
    //                        ferramentariaCell.classList.add('text-center', 'align-middle');
    //                        ferramentariaCell.textContent = ferramentaria.nome;

    //                        const catalogoCell = document.createElement('td');
    //                        catalogoCell.classList.add('text-center', 'align-middle');
    //                        catalogoCell.textContent = product.classType;

    //                        const classeCell = document.createElement('td');
    //                        classeCell.classList.add('text-center', 'align-middle');
    //                        classeCell.textContent = product.tipo; 

    //                        const tipoCell = document.createElement('td');
    //                        tipoCell.classList.add('text-center', 'align-middle');
    //                        tipoCell.textContent = product.classe;

    //                        const codigoCell = document.createElement('td');
    //                        codigoCell.classList.add('text-center', 'align-middle');
    //                        codigoCell.textContent = product.codigo;

    //                        const itemCell = document.createElement('td');
    //                        itemCell.classList.add('text-center', 'align-middle');
    //                        itemCell.textContent = product.nome;

    //                        const saldoCell = document.createElement('td');
    //                        saldoCell.classList.add('text-center', 'align-middle');
    //                        saldoCell.textContent = ferramentaria.quantity; 

    //                        const reservedCell = document.createElement('td');
    //                        reservedCell.classList.add('text-center', 'align-middle');
    //                        reservedCell.textContent = ferramentaria.reservedQuantity;

    //                        const totalCell = document.createElement('td');
    //                        totalCell.classList.add('text-center', 'align-middle');
    //                        totalCell.textContent = ferramentaria.availableQuantity;

    //                        const controleCell = document.createElement('td');
    //                        controleCell.classList.add('text-center', 'align-middle');
    //                        controleCell.textContent = product.porType;

    //                        const viewCell = document.createElement('td');
    //                        viewCell.classList.add('text-center', 'align-middle');

    //                        if (ferramentaria.reservedQuantity > 0) {
    //                            const viewLink = document.createElement("a");
    //                            viewLink.setAttribute("href", "#");
    //                            viewLink.textContent = 'View';

    //                            viewLink.onclick = function (e) {
    //                                e.preventDefault();

    //                                openReservationDetailModal(product.id, product.codigo, ferramentaria.nome);
    //                            };

    //                            viewCell.appendChild(viewLink);
    //                        }
                                                   
    //                        row.appendChild(ferramentariaCell);
    //                        row.appendChild(catalogoCell);
    //                        row.appendChild(classeCell);
    //                        row.appendChild(tipoCell);
    //                        row.appendChild(codigoCell);
    //                        row.appendChild(itemCell);
    //                        row.appendChild(saldoCell);
    //                        row.appendChild(reservedCell);
    //                        row.appendChild(totalCell);
    //                        row.appendChild(controleCell);
    //                        row.appendChild(viewCell);
    //                        tbody.appendChild(row);
    //                    });

    //                });

    //            } else {
    //                appendAlertWithoutAnimation('No result found', 'warning');
    //                SearchButton.disabled = false;
    //                loadingSpan.classList.add('visually-hidden');
    //            }

    //        })
    //        .catch(error => {
    //            console.error('AJAX error:', error);
    //            appendAlertWithoutAnimation('Failed to fetch data.', 'danger');
    //            SearchButton.disabled = false;
    //            loadingSpan.classList.add('visually-hidden');
    //        });





    //   /* this.submit();*/
    //}
});

const loadingGestor = () => {

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
    });

    document.querySelectorAll('.form-select').forEach(select => {
        select.style.pointerEvents = 'none';
        select.style.backgroundColor = '#f8f9fa';
    });

    document.querySelectorAll('.btn').forEach(button => {
        button.disabled = true;
    });

    document.getElementById('checkReservadoRetirada').style.pointerEvents = 'none';
    document.getElementById('checkReservadoRetirada').style.backgroundColor = '#f8f9fa';

    

    return () => {

        document.querySelectorAll('.form-control').forEach(input => {
            input.readOnly = false;
        });

        document.querySelectorAll('.form-select').forEach(select => {
            select.style.pointerEvents = '';
            select.style.backgroundColor = '';
        });

        document.querySelectorAll('.btn').forEach(button => {
            button.disabled = false;
        });

        document.getElementById('checkReservadoRetirada').style.pointerEvents = '';
        document.getElementById('checkReservadoRetirada').style.backgroundColor = '';

    }
}

const populateGestorTable = (data) => {

    const tbody = document.getElementById('gestorRRBody');
    tbody.innerHTML = '';

    data.forEach((product) => {

        product.ferramentarias.forEach((ferramentaria) => {

            const row = document.createElement('tr');

            const ferramentariaCell = document.createElement('td');
            ferramentariaCell.classList.add('text-center', 'align-middle');
            ferramentariaCell.textContent = ferramentaria.nome;

            const catalogoCell = document.createElement('td');
            catalogoCell.classList.add('text-center', 'align-middle');
            catalogoCell.textContent = product.classType;

            const classeCell = document.createElement('td');
            classeCell.classList.add('text-center', 'align-middle');
            classeCell.textContent = product.tipo;

            const tipoCell = document.createElement('td');
            tipoCell.classList.add('text-center', 'align-middle');
            tipoCell.textContent = product.classe;

            const codigoCell = document.createElement('td');
            codigoCell.classList.add('text-center', 'align-middle');
            codigoCell.textContent = product.codigo;

            const itemCell = document.createElement('td');
            itemCell.classList.add('text-center', 'align-middle');
            itemCell.textContent = product.nome;

            const saldoCell = document.createElement('td');
            saldoCell.classList.add('text-center', 'align-middle');
            saldoCell.textContent = ferramentaria.quantity;

            const reservedCell = document.createElement('td');
            reservedCell.classList.add('text-center', 'align-middle');
            reservedCell.textContent = ferramentaria.reservedQuantity;

            const totalCell = document.createElement('td');
            totalCell.classList.add('text-center', 'align-middle');
            totalCell.textContent = ferramentaria.availableQuantity;

            const controleCell = document.createElement('td');
            controleCell.classList.add('text-center', 'align-middle');
            controleCell.textContent = product.porType;

            const viewCell = document.createElement('td');
            viewCell.classList.add('text-center', 'align-middle');

            if (ferramentaria.reservedQuantity > 0) {
                const viewLink = document.createElement("a");
                viewLink.setAttribute("href", "#");
                viewLink.textContent = 'View';

                viewLink.onclick = function (e) {
                    e.preventDefault();

                    openReservationDetailModal(product.id, product.codigo, ferramentaria.nome);
                };

                viewCell.appendChild(viewLink);
            }

            row.appendChild(ferramentariaCell);
            row.appendChild(catalogoCell);
            row.appendChild(classeCell);
            row.appendChild(tipoCell);
            row.appendChild(codigoCell);
            row.appendChild(itemCell);
            row.appendChild(saldoCell);
            row.appendChild(reservedCell);
            row.appendChild(totalCell);
            row.appendChild(controleCell);
            row.appendChild(viewCell);
            tbody.appendChild(row);
        });

    });

}


const openReservationDetailModal = async (idCatalogo, codigo, ferramentariaNome) => {

    try {

        const url = `/GestorRetiradaReserva/GetReservationDetails?IdCatalogo=${idCatalogo}`;

        const result = await fetchJson(url);

        if (result.length == 0) throw new Error('Nenhum resultado encontrado.');

        populateReservationTable(result, codigo, ferramentariaNome);


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


    //console.log(idCatalogo);

    //fetch(`/GestorRetiradaReserva/GetReservationDetails?IdCatalogo=${idCatalogo}`, {
    //    method: 'GET',
    //    headers: {
    //        'Accept': 'application/json' // Optional: tells server you expect JSON
    //    }
    //})
    //    .then(response => {
    //        if (!response.ok) throw new Error("Network response was not ok");
    //        return response.json();
    //    }).then(data => {

    //        console.log(data);

    //        if (data.success) {
    //            populateReservationTable(data.reservationResult, codigo, ferramentariaNome);
    //            console.log('data result', data.reservationResult);
    //        } else {
    //            appendAlertWithoutAnimation('Nenhum resultado encontrado', 'warning');
    //        }

    //    })
    //    .catch(error => {
    //        console.error('AJAX error:', error);
    //        appendAlertWithoutAnimation('Failed to fetch data.', 'danger');
    //    });


    //if (!response.ok) {
    //    throw new Error(`HTTP error! Status: ${response.status}`);
    //}

    //const data = await response.json();

    //console.log(data);

    //fetch('/GestorRetiradaReserva/GetGestorReservationRetirada', {
    //    method: 'POST',
    //    headers: {
    //        'Content-Type': 'application/json'
    //    },
    //    body: JSON.stringify(formData)
    //})

}

const populateReservationTable = (reservations, codigo, ferramentariaNome) => {

    const modal = new bootstrap.Modal(document.getElementById('reservationDetailModal'));

    const modalElement = document.getElementById('reservationDetailModal');

    modalElement.querySelector('#exampleModalLabelTitle').textContent = `${codigo} - ${ferramentariaNome}`;

    const tbody = document.getElementById('reservationCompleteTableBody');
    tbody.innerHTML = '';

    reservations.forEach(item => {

        const row = document.createElement('tr');

        const idNoCell = document.createElement('td');
        idNoCell.className = 'text-center align-middle';
        idNoCell.textContent = item.idReservation;

        const orderNoCell = document.createElement('td');
        orderNoCell.className = 'text-center align-middle';
        orderNoCell.textContent = item.orderNo;

        const memberCell = document.createElement('td');
        memberCell.className = 'text-center align-middle';
        memberCell.textContent = item.memberInfo.chapa;

        const memberNomeCell = document.createElement('td');
        memberNomeCell.className = 'text-center align-middle';
        memberNomeCell.textContent = item.memberInfo.nome;

        const memberFuncaoCell = document.createElement('td');
        memberFuncaoCell.className = 'text-center align-middle';
        memberFuncaoCell.textContent = item.memberInfo.funcao;

        const leaderCell = document.createElement('td');
        leaderCell.className = 'text-center align-middle';
        leaderCell.textContent = item.leaderInfo.chapa;

        const leaderNomeCell = document.createElement('td');
        leaderNomeCell.className = 'text-center align-middle';
        leaderNomeCell.textContent = item.leaderInfo.nome;

        const leaderFuncaoCell = document.createElement('td');
        leaderFuncaoCell.className = 'text-center align-middle';
        leaderFuncaoCell.textContent = item.leaderInfo.funcao;

        const qtdCell = document.createElement('td');
        qtdCell.className = 'text-center align-middle';
        qtdCell.textContent = item.quantidade;

        const tipoCell = document.createElement('td');
        tipoCell.className = 'text-center align-middle';
        tipoCell.textContent = item.reservationType;

        const statusCell = document.createElement('td');
        statusCell.className = 'text-center align-middle';
        statusCell.textContent = item.statusString;

        function formatDateTime(date) {
            const day = String(date.getDate()).padStart(2, '0');
            const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
            const year = String(date.getFullYear()).slice(-2);
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');

            return `${day}/${month}/${year} ${hours}:${minutes}`;
        }


        const dataRegistroCell = document.createElement('td');
        dataRegistroCell.className = 'text-center align-middle';

        const date = new Date(item.dateReservation)

        dataRegistroCell.textContent = formatDateTime(date);

        row.appendChild(idNoCell);
        row.appendChild(orderNoCell);
        row.appendChild(memberCell);
        row.appendChild(memberNomeCell);
        row.appendChild(memberFuncaoCell);
        row.appendChild(leaderCell);
        row.appendChild(leaderNomeCell);
        row.appendChild(leaderFuncaoCell);
        row.appendChild(qtdCell);
        row.appendChild(tipoCell);
        row.appendChild(statusCell);
        row.appendChild(dataRegistroCell);
        tbody.appendChild(row);

    });

    modal.show();

}