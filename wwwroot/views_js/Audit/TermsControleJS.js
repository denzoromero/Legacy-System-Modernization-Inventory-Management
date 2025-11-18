


document.getElementById("inputChapaNome").addEventListener("keydown", async function (event) {
    if (event.key === "Enter") {

        cleanAllArea();

        let cleanup;
        try {

            let inputValue = event.target.value;
            if (!inputValue) throw new ValidationError('Por favor insira chapa/nome');

            cleanup = loadingAudit('loadingSearchEmployee', 'searchImg');

            let url = `/Auditting/RetrieveEmployeeInformation?givenInformation=${inputValue}`;

            const result = await fetchJson(url);

            if (result.length == 1) {
                event.target.value.value = '';

                let urlTerms = `/Auditting/GetTermsData?CodPessoa=${result[0].codPessoa}`;

                const resultTerms = await fetchJson(urlTerms);

                showInformation(result[0]);
                populateTable(resultTerms);

            } else {
                event.target.value.value = '';
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



const showInformation = (employee) => {


    if (employee.imageString !== null) {
        document.getElementById("imgEmployee").src = employee.imageString;
    } else {
        document.getElementById("imgEmployee").src = "/img/image-not-available.jpg";
    }

    document.getElementById("inputHiddenCodPessoa").value = employee.codPessoa;
    /*document.getElementById("employeeChapa").inn = employee.chapa;*/
    //document.getElementById("employeeChapa").value = employee.chapa;
    //document.getElementById("employeeCodColigada").value = employee.codColigada;
    //document.getElementById("inputCodPessoa").value = employee.codPessoa;
    //document.getElementById("inputIdTerceiro").value = employee.idTerceiro;

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


        let urlTerms = `/Auditting/GetTermsData?CodPessoa=${result.codPessoa}`;

        const resultTerms = await fetchJson(urlTerms);


        showInformation(result);
        populateTable(resultTerms);

        if (cleanup) cleanup();

        const modalElement = document.getElementById('multipleEmployeeModal');
        const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

        modal.hide();



    } catch (error) {

        if (cleanup) cleanup();

        const modalElement = document.getElementById('multipleEmployeeModal');
        const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

        modal.hide();

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


const loadingAudit = (loadingSpan, imgId) => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
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

        document.getElementById(loadingSpan).classList.add('visually-hidden');
        document.getElementById(imgId).classList.remove('visually-hidden');

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





const cleanAllArea = () => {

    document.getElementById("imgEmployee").src = "/img/image-not-available.jpg";
    document.getElementById("labelChapa").innerText = '';
    document.getElementById("labelNome").innerText = '';
    document.getElementById("labelSituacao").innerText = '';
    document.getElementById("labelFuncao").innerText = '';
    document.getElementById("labelSecao").innerText = '';
    document.getElementById("consultEmployeeTable").querySelector("tbody").innerHTML = '';
    document.getElementById("inputHiddenCodPessoa").value = '';

}




//document.getElementById('formSearchEmployee').addEventListener("submit", async function (e) {
//    e.preventDefault();

//    try {

//        let inputValue = document.getElementById('filterNameId').value;

//        let url = `/Auditting/GetTermsData?givenInformation=${inputValue}`;

//        const result = await fetchJson(url);

//        populateTable(result);

//    } catch (error) {

//        /*if (cleanup) cleanup();*/
//        console.error(error);
//        if (error.status === 401 || error.status === 403) {
//            handleAuthenticationError(error);
//        } else if (error instanceof ValidationError) {
//            appendAlert(error.message, 'warning');
//        } else {
//            appendAlertWithoutAnimation(error.message, 'danger');
//        }

//    }

//});


const populateTable = (result) => {

    const tbody = document.getElementById("consultEmployeeTable").querySelector("tbody");
    tbody.innerHTML = '';

    const row = document.createElement('tr');

    //const chapaCell = document.createElement('td');
    //chapaCell.className = 'text-center align-middle';
    //chapaCell.textContent = result.chapa;

    //const nomeCell = document.createElement('td');
    //nomeCell.className = 'text-center align-middle';
    //nomeCell.textContent = result.nome;

    const responsavelCell = document.createElement('td');
    responsavelCell.className = 'text-center align-middle';
    responsavelCell.textContent = result.responsavel;

    const dateCell = document.createElement('td');
    dateCell.className = 'text-center align-middle';
    dateCell.textContent = result.dataRegistroString;

    const emptyCell = document.createElement('td');

    const selectBtn = document.createElement('a');
    selectBtn.className = 'btn btn-link align-items-center btnSelect';

    if (result.imageData) {

        selectBtn.textContent = "View PDF";
        selectBtn.href = "#"; 
        selectBtn.style.cursor = "pointer";
        selectBtn.onclick = function (e) {
            e.preventDefault();
            viewPdfFromBytes(result.imageData);
        };


    } else {

        //selectBtn.setAttribute('data-bs-toggle', 'modal');
        //selectBtn.setAttribute('data-bs-target', '#exampleModal');
        selectBtn.textContent = "Upload PDF";
        selectBtn.onclick = function () {

            openModalForUpload(result);

        };

    }
    emptyCell.appendChild(selectBtn);


    //row.appendChild(chapaCell);
    //row.appendChild(nomeCell);
    row.appendChild(responsavelCell);
    row.appendChild(dateCell);
    row.appendChild(emptyCell);
    tbody.appendChild(row);

};


const openModalForUpload = (term) => {

    document.getElementById('employeeChapaTitleHolder').textContent = document.getElementById('labelChapa').innerText;
    document.getElementById('inputHiddenIdTerms').value = term.id;

    const modal = document.getElementById('exampleModal');
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();

}

document.getElementById('formUploadPDF').addEventListener("submit", async function (e) {

    e.preventDefault();

    try {

        let inputValue = document.getElementById('inputHiddenIdTerms').value;
        if (!inputValue) throw new ValidationError('No IdTerm.');

        const fileInput = document.getElementById('formFile');
        const file = fileInput.files[0];
        if (!file) throw new ValidationError('Please select a image file to upload.');

        if (file.type !== 'application/pdf') throw new ValidationError('Inserted file is not an pdf.');

        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) throw new ValidationError('File size too large.');

        if (file.size === 0) throw new ValidationError('The selected file appears to be empty.');

        let codPessoa = document.getElementById('inputHiddenCodPessoa').value;

        console.log('codpessoa', codPessoa);

        const form = e.target;
        const formData = new FormData(form);
        formData.append('codPessoa', codPessoa);

        const result = await fetchJson(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'Accept': 'application/json'
            }
        });

        let urlTerms = `/Auditting/GetTermsData?CodPessoa=${codPessoa}`;

        const resultTerms = await fetchJson(urlTerms);

        populateTable(resultTerms);

        appendAlert('Upload Success', 'success');

        const modalElement = document.getElementById('exampleModal');
        const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

        modal.hide();


        //const formData = new FormData();
        //formData.append('pdfFile', file);
        //formData.append('idTerms', inputValue);

        //for (let [key, value] of formData.entries()) {
        //    console.log(key, value);
        //}

        //const form = e.target;
        //const actionUrl = form.getAttribute('action');


        //const result = await fetchJson(actionUrl, {
        //    method: 'POST',
        //    body: formData,
        //    //headers: {
        //    //    /*'Content-Type': 'multipart/form-data',*/
        //    //    'Accept': 'application/json'
        //    //}
        //});


        

    } catch (error) {

        /*if (cleanup) cleanup();*/
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


function downloadPdfFromBytes(pdfBytes, filename) {
    try {
        // Convert base64 string to blob if needed, or use directly if it's already bytes
        let blob;

        if (typeof pdfBytes === 'string') {
            // If it's a base64 string
            const binaryString = atob(pdfBytes);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }
            blob = new Blob([bytes], { type: 'application/pdf' });
        } else if (pdfBytes instanceof ArrayBuffer || Array.isArray(pdfBytes)) {
            // If it's already bytes (ArrayBuffer or number array)
            blob = new Blob([pdfBytes], { type: 'application/pdf' });
        } else {
            throw new Error('Unsupported PDF data format');
        }

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = filename;

        document.body.appendChild(a);
        a.click();

        // Clean up
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        console.log('PDF downloaded successfully');

    } catch (error) {
        console.error('Download failed:', error);
        appendAlert('Failed to download PDF', 'danger');
    }
}


function viewPdfFromBytes(pdfBytes) {
    try {
        let blob;

        if (typeof pdfBytes === 'string') {
            // Base64 string
            const binaryString = atob(pdfBytes);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }
            blob = new Blob([bytes], { type: 'application/pdf' });
        } else {
            // Raw bytes
            blob = new Blob([pdfBytes], { type: 'application/pdf' });
        }

        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');

        // Optional: Revoke URL after some time (or let browser handle it)
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);

    } catch (error) {
        console.error('View PDF failed:', error);
        appendAlert('Failed to open PDF', 'danger');
    }
}