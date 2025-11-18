
document.addEventListener('DOMContentLoaded', function () {
    const selectYear = document.getElementById('selectYear');
    const currentYear = new Date().getFullYear();
    const startYear = 2000;
    /*const endYear = 2025;*/
    const endYear = new Date().getFullYear();;

    let optionsHTML = '';
    for (let year = startYear; year <= endYear; year++) {
        const selected = year === currentYear ? 'selected' : '';
        optionsHTML += `<option value="${year}" ${selected}>${year}</option>`;
    }

    selectYear.innerHTML = optionsHTML;
});

document.getElementById("inputChapaNome").addEventListener("keydown", async function (event) {
    if (event.key === "Enter") {

        let cleanup;
        try {

            let inputValue = event.target.value;
            if (!inputValue) throw new ValidationError('Por favor insira chapa/nome');

            cleanup = loadingAudit('loadingSearchEmployee', 'searchImg');

            let url = `/Auditting/RetrieveEmployeeInformation?givenInformation=${inputValue}`;

            const result = await fetchJson(url);

            if (result.length == 1) {
                showInformation(result[0]);
                document.getElementById('inputChapaNome').value = '';
            } else {
                document.getElementById('searchChapaName').value = '';
                showModalEmployeeInformation(result);
            }

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

    }
});


document.getElementById('btnSearchEmployee').addEventListener('click', async function () {

    let cleanup;
    try {

        let inputValue = document.getElementById('inputChapaNome').value;
        if (!inputValue) throw new ValidationError('Por favor insira chapa/nome');

        cleanup = loadingAudit('loadingSearchEmployee', 'searchImg');

        let url = `/Auditting/RetrieveEmployeeInformation?givenInformation=${inputValue}`;

        const result = await fetchJson(url);

        if (result.length == 1) {
            showInformation(result[0]);
            document.getElementById('inputChapaNome').value = '';
        } else {
            document.getElementById('searchChapaName').value = '';
            showModalEmployeeInformation(result);
        }

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

});

const loadingAudit = (loadingSpan, imgId) => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
    });

    document.querySelectorAll('.form-select').forEach(select => {
        select.style.pointerEvents = 'none';
        select.style.backgroundColor = '#f8f9fa';
    });

    document.getElementById(loadingSpan).classList.remove('visually-hidden');
    document.getElementById(imgId).classList.add('visually-hidden');

    return () => {

        document.querySelectorAll('.btn').forEach(btn => {
            btn.disabled = false;
        });

        document.querySelectorAll('.form-control').forEach(input => {
            input.readOnly = false;
        });

        document.querySelectorAll('.form-select').forEach(select => {
            select.style.pointerEvents = '';
            select.style.backgroundColor = '';
        });

        document.getElementById(loadingSpan).classList.add('visually-hidden');
        document.getElementById(imgId).classList.remove('visually-hidden');

    }
}

const showInformation = (employee) => {


    if (employee.imageString !== null) {
        document.getElementById("imgEmployee").src = employee.imageString;
    } else {
        document.getElementById("imgEmployee").src = "/img/image-not-available.jpg";
    }

    document.getElementById("employeeChapa").value = employee.chapa;
    document.getElementById("employeeCodColigada").value = employee.codColigada;
    document.getElementById("inputCodPessoa").value = employee.codPessoa;
    document.getElementById("inputIdTerceiro").value = employee.idTerceiro;

    document.getElementById("labelChapa").innerText = employee.chapa;
    document.getElementById("labelNome").innerText = employee.nome;

    document.getElementById("labelSituacao").innerText = employee.codSituacao;
    if (employee.codSituacao === "D") {
        document.getElementById("labelSituacao").classList.add('text-danger');
        document.getElementById("labelSituacao").classList.add('fw-bold');
    } else {
        document.getElementById("labelSituacao").classList.remove('text-danger');
        document.getElementById("labelSituacao").classList.remove('fw-bold');
    }

    document.getElementById("labelFuncao").innerText = employee.funcao;
    document.getElementById("labelSecao").innerText = employee.secao;

}

const showModalEmployeeInformation = (employees) => {

    const modal = new bootstrap.Modal(document.getElementById('multipleEmployeeModal'));
    const modalElement = document.getElementById('multipleEmployeeModal');

    const tableBody = modalElement.querySelector("#employeeListBody");
    tableBody.innerHTML = '';

    employees.forEach((employee) => {

        const row = document.createElement('tr');

        const selectBtnCell = document.createElement('td');
        const selectBtn = document.createElement('a');
        selectBtn.className = 'btn btn-link align-items-center btnSelect';
        selectBtn.textContent = "Select";
        selectBtn.addEventListener('click', (e) => {
            e.preventDefault();
            selectEmployee(employee, selectBtn);
        });
        selectBtnCell.appendChild(selectBtn);

        const chapaCell = document.createElement('td');
        /*nomeCell.className = 'text-center align-middle';*/
        chapaCell.className = 'chapaemployee';
        chapaCell.textContent = employee.chapa;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'nomeemployee';
        nomeCell.textContent = employee.nome;

        const secaoCell = document.createElement('td');
        secaoCell.textContent = employee.secao;

        const funcaoCell = document.createElement('td');
        funcaoCell.textContent = employee.funcao;

        row.appendChild(selectBtnCell);
        row.appendChild(chapaCell);
        row.appendChild(nomeCell);
        row.appendChild(secaoCell);
        row.appendChild(funcaoCell);
        tableBody.appendChild(row);
    });

    modal.show();

} 

const selectEmployee = async (employee, selectBtn) => {

    let cleanup;
    try {
  
        cleanup = loadingSelectEmployee(selectBtn);

        let url;
        if (employee.idTerceiro == 0) {
            url = `/Auditting/SelectedEmployee?codPessoa=${employee.codPessoa}`;
        } else {
            url = `/Auditting/SelectedThirdParty?idTerceiro=${employee.idTerceiro}`;
        }

        const result = await fetchJson(url);

        showInformation(result);
        document.getElementById('inputChapaNome').value = '';
            
        if (cleanup) cleanup();

        const modalElement = document.getElementById('multipleEmployeeModal');
        const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

        modal.hide();

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

}

const loadingSelectEmployee = (selectBtn) => {

    document.querySelectorAll('.btnSelect').forEach(select => {
        select.style.pointerEvents = 'none';
        select.style.backgroundColor = '#f8f9fa';
    });

    selectBtn.setAttribute('data-original-html', selectBtn.innerHTML);
    selectBtn.setAttribute('data-original-text', selectBtn.textContent);

    selectBtn.innerHTML = '<span class="spinner-grow text-success me-3"></span>';

    const modalElement = document.getElementById('multipleEmployeeModal');
    const closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"], .btn-close');
    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

    /*const modalElement = modal._element;*/

    const preventHide = (e) => {
        e.preventDefault();
        return false;
    };

    modalElement.addEventListener('hide.bs.modal', preventHide);
    closeButtons.forEach(btn => btn.style.pointerEvents = 'none');


    return () => {

        document.querySelectorAll('.btnSelect').forEach(select => {
            select.style.pointerEvents = '';
            select.style.backgroundColor = '';
        });

        selectBtn.innerHTML = selectBtn.getAttribute('data-original-html');

        selectBtn.removeAttribute('data-original-html');
        selectBtn.removeAttribute('data-original-text');

        modalElement.removeEventListener('hide.bs.modal', preventHide);
        closeButtons.forEach(btn => btn.style.pointerEvents = 'auto');

    }
}


document.getElementById('formGenerateReport').addEventListener("submit",async function (e) {
    e.preventDefault();

    let cleanup;
    try {

        let inputValue = document.getElementById('employeeChapa').value;
        if (!inputValue) throw new ValidationError('Por favor insira chapa/nome');

        let coligadaValue = document.getElementById('employeeCodColigada').value;
        if (!coligadaValue) throw new ValidationError('No coligada information.');

        let selectedYearValue = document.getElementById('selectYear').value;
        if (!selectedYearValue) throw new ValidationError('Please select year.');

        cleanup = loadingGeneratePDF();

        const form = e.target;
        const formData = new FormData(form);

        /*selectPdf*/
        /*form.submit();*/

        const result = await fetchJson(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                /*'Content-Type': 'multipart/form-data',*/
                'Accept': 'application/json'
            }
        });

        //await new Promise((resolve) => {
        //    var printWindow = window.open('', '_blank');

        //    printWindow.onload = function () {
        //        printWindow.document.open();
        //        printWindow.document.write(result);
        //        printWindow.document.close();
        //        resolve();
        //    };
        //});

        //// Wait a bit for content to render
        //await new Promise(resolve => setTimeout(resolve, 500));
        //printWindow.print();

        if (cleanup) cleanup();

        var printWindow = window.open('', '_blank');
        printWindow.document.open();
        printWindow.document.write(result);
        printWindow.document.close();
        printWindow.print();



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
    
});

const loadingGeneratePDF = () => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
    });

    document.querySelectorAll('.form-select').forEach(select => {
        select.style.pointerEvents = 'none';
        select.style.backgroundColor = '#f8f9fa';
    });

    document.getElementById('loadingGeneratePDF').classList.remove('visually-hidden');
    document.getElementById('svgPrinter').classList.add('visually-hidden');

    return () => {

        document.querySelectorAll('.btn').forEach(btn => {
            btn.disabled = false;
        });

        document.querySelectorAll('.form-control').forEach(input => {
            input.readOnly = false;
        });

        document.querySelectorAll('.form-select').forEach(select => {
            select.style.pointerEvents = '';
            select.style.backgroundColor = '';
        });

        document.getElementById('loadingGeneratePDF').classList.add('visually-hidden');
        document.getElementById('svgPrinter').classList.remove('visually-hidden');

    }

}




document.getElementById("searchChapaName").addEventListener("keyup", function (e) {

    let inputtedValue = e.target.value.toUpperCase();

    var table = document.getElementById("employeeListBody");
    var tr = table.getElementsByTagName("tr");

    for (var i = 0; i < tr.length; i++) {

        var chapaTd = tr[i].getElementsByClassName("chapaemployee")[0];
        var nomeTd = tr[i].getElementsByClassName("nomeemployee")[0];

        var chapaTxtValue = chapaTd ? (chapaTd.textContent || chapaTd.innerText) : "";
        var nomeTxtValue = nomeTd ? (nomeTd.textContent || nomeTd.innerText) : "";

        if (chapaTxtValue.toUpperCase().indexOf(inputtedValue) > -1 || nomeTxtValue.toUpperCase().indexOf(inputtedValue) > -1) {
            tr[i].style.display = "";
        } else {
            tr[i].style.display = "none";
        }
    }

});
