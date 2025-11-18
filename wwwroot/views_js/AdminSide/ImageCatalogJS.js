

let currentPage = 0;


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




document.getElementById('btnSubmitSearch').addEventListener('click', async function (event) {

    currentPage = 0;
    GetCatalogResult();

});

const GetCatalogResult = async () => {

    let cleanup;
    try {

        const inputCodigo = document.getElementById('inputCodigo').value;
        const inputItem = document.getElementById('inputItem').value;
        const ddlCatalogoValue = document.getElementById('ddlCatalogo').value;
        const ddlClasseValue = document.getElementById('ddlClasse').value;
        const ddlTipoValue = document.getElementById('ddlTipo').value;

        const formData = {
            Codigo: inputCodigo ? inputCodigo : null,
            Item: inputItem ? inputItem : null,
            CategoriaClasse: ddlCatalogoValue ? parseInt(ddlCatalogoValue) : null,
            Id: ddlClasseValue ? parseInt(ddlClasseValue) : null,
            IdCategoria: ddlTipoValue ? parseInt(ddlTipoValue) : null,
            CurrentPage: currentPage
        };

        const queryParams = new URLSearchParams();
        Object.keys(formData).forEach(key => {
            if (formData[key] !== null && formData[key] !== undefined) {
                queryParams.append(key, formData[key]);
            }
        });

        const url = `/AdminSide/SearchCatalog?${queryParams.toString()}`;

        const result = await fetchJson(url);

        if (!result) throw new ValidationError('No result found.');


        populateCatalogTable(result.catalogResult);
        updatePaginationControl(result.totalPages);


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


const populateCatalogTable = (catalogs) => {

    const tbody = document.querySelector('#catalogTable');
    tbody.innerHTML = '';

    catalogs.forEach(item => {

        const row = document.createElement('tr');

        const idCell = document.createElement('td');
        idCell.className = 'text-center align-middle';
        idCell.textContent = item.id;

        const codigoCell = document.createElement('td');
        codigoCell.className = 'text-center align-middle';
        codigoCell.textContent = item.codigo;

        const itemCell = document.createElement('td');
        itemCell.className = 'text-center align-middle';
        itemCell.textContent = item.nome;

        const blankCell = document.createElement('td');
        blankCell.className = 'text-center align-middle';

        const viewLink = document.createElement("a");
        viewLink.href = "#";
        viewLink.className = 'btn btn-info';
        viewLink.innerText = "View";
        viewLink.onclick = function () {
            openCatalogModal(item);
        };

        blankCell.appendChild(viewLink);

        row.appendChild(idCell);
        row.appendChild(codigoCell);
        row.appendChild(itemCell);
        row.appendChild(blankCell);
        tbody.appendChild(row);

    });

}

const updatePaginationControl = (totalPageCount) => {

    //if (totalPageCount != 0) {
    //    document.getElementById('paginationDiv').classList.remove('visually-hidden');
    //} else {
    //    document.getElementById('paginationDiv').classList.add('visually-hidden');
    //}

    document.getElementById('paginationDiv').classList.toggle('visually-hidden', totalPageCount === 0);

    if (totalPageCount !== 0) {

        const paginationControls = document.getElementById('paginationControls');
        paginationControls.innerHTML = '';

        const prevButton = document.createElement('li');
        prevButton.className = 'page-item' + (currentPage === 0 ? ' disabled' : '');
        prevButton.innerHTML = `<a class="page-link" href="#">Previous</a>`;
        prevButton.onclick = function (event) {
            event.preventDefault(); // Prevent default action
            if (currentPage > 0) {
                currentPage--;
                GetCatalogResult();
            }
        };
        paginationControls.appendChild(prevButton);

        const nextButton = document.createElement('li');
        nextButton.className = 'page-item' + (currentPage === totalPageCount - 1 ? ' disabled' : '');
        nextButton.innerHTML = `<a class="page-link" href="#">Next</a>`;
        nextButton.onclick = function (event) {
            event.preventDefault(); // Prevent default action
            if (currentPage < totalPageCount - 1) {
                currentPage++;
                GetCatalogResult();
            }
        };
        paginationControls.appendChild(nextButton);


    }
    
}



const openCatalogModal = async (item) => {

    try {
        
        let url = `/AdminSide/CheckImage?IdCatalogo=${item.id}`;

        const result = await fetchJson(url);

        const modal = new bootstrap.Modal(document.getElementById('exampleModal'));

        if (result !== "") {
            console.log('image found');
            document.getElementById("imgCatalog").src = result;
        } else {
            console.log('no image found');
            document.getElementById("imgCatalog").src = "/img/image-not-available.jpg";
        }

        document.getElementById("inputIdCatalogo").value = item.id;

        document.getElementById("labelCodigo").innerText = item.codigo;
        document.getElementById("labelItem").innerText = item.nome;

        modal.show();


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


    //const modal = new bootstrap.Modal(document.getElementById('exampleModal'));
    ///*const modalElement = document.getElementById('transferModal');*/

    //if (item.imageByteString !== "") {
    //    console.log('image found');
    //    document.getElementById("imgCatalog").src = item.imageByteString;
    //} else {
    //    console.log('no image found');
    //    document.getElementById("imgCatalog").src = "/img/image-not-available.jpg";
    //}

    //document.getElementById("inputIdCatalogo").value = item.id;

    ////document.getElementById("labelCatalogo").innerText = item.nome;
    ////document.getElementById("labelClasse").innerText = item.nome;
    ////document.getElementById("labelTipo").innerText = item.nome;
    //document.getElementById("labelCodigo").innerText = item.codigo;
    //document.getElementById("labelItem").innerText = item.nome;

    //modal.show();

}


document.getElementById('formInsertImage').addEventListener("submit", async function (e) {
    e.preventDefault();

    try {

        let inputValue = document.getElementById('inputIdCatalogo').value;
        if (!inputValue) throw new ValidationError('No IdCatalogo.');

        const fileInput = document.getElementById('formFile');
        const file = fileInput.files[0];
        if (!file) throw new ValidationError('Please select a image file to upload.');

        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/pjpeg'];
        if (!allowedTypes.includes(file.type)) {
            throw new ValidationError('Please select a JPEG image file (jpg, jpeg).');
        }

        // Optional: Also check file extension as backup
        const allowedExtensions = ['.jpg', '.jpeg', '.jpe', '.jfif'];
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
        if (!allowedExtensions.includes(fileExtension)) {
            throw new ValidationError('File must be a JPEG image (jpg, jpeg).');
        }

        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) throw new ValidationError('File size too large.');

        if (file.size === 0) throw new ValidationError('The selected file appears to be empty.');

        const form = e.target;
        const formData = new FormData(form);

        const result = await fetchJson(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!result) throw new ValidationError('Error in uploading Image.');

        document.getElementById("imgCatalog").src = result;

        appendAlert('Upload Success', 'success');


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