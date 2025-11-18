





document.getElementById('btnSearchLeader').addEventListener('click', function (event) {
    const chapanomeLeader = document.getElementById("filterSearchLeader").value;

    GetLeaderInfo(chapanomeLeader);

    //if (chapanomeLeader !== "") {
    //    GetLeaderInfo(chapanomeLeader);
    //} else {
    //    appendAlertWithoutAnimation('Filter is empty', 'warning');
    //}
});

document.getElementById("filterSearchLeader").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {

        GetLeaderInfo(event.target.value);

        //if (chapanomeLeader !== "") {
        //    GetLeaderInfo(chapanomeLeader);
        //} else {
        //    appendAlertWithoutAnimation('Filter is empty', 'warning');
        //}
    }
});


const GetLeaderInfo = async (chapanomeLeader) => {

    let cleanup;
    try {

        /*if (!chapanomeLeader) throw new ValidationError('Por favor insira chapa/nome');*/

        cleanup = loadingSearchLeader('loadingSearchLeader');

        let url = `/AdminSide/GetLeaderInformation?givenInfo=${chapanomeLeader}`;

        const result = await fetchJson(url);

        makeLeaderTable(result);

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

    //$.ajax({
    //    url: '/AdminSide/GetLeaderInformation',
    //    type: 'GET',
    //    data: { givenInfo: chapanomeLeader },
    //    success: function (data) {

    //        if (data.success) {
    //            if (data.leaderInfo) {
    //                console.log(data.leaderInfo);
    //                makeLeaderTable(data.leaderInfo);
    //            } else {
    //                console.log(data.message);
    //                appendAlertWithoutAnimation(data.message, 'danger');
    //            }
    //        } else {
    //            console.log(data.message);
    //            clearLeaderForm();
    //            appendAlertWithoutAnimation(data.message, 'danger');
    //        }
    //    },
    //    error: function (error) {
    //        console.error('Error fetching items:', error);
    //        appendAlertWithoutAnimation(error, 'danger');
    //    }
    //});

}

const makeLeaderTable = (leaderInfo) => {

    const tbody = document.getElementById("leaderTableBody");
    tbody.innerHTML = '';

    leaderInfo.forEach((info, index) => {

        const row = document.createElement('tr');


        const idLeaderCell = document.createElement('td');
        idLeaderCell.className = 'text-center align-middle';
        idLeaderCell.textContent = info.idLeader;

        const chapaCell = document.createElement('td');
        chapaCell.className = 'text-center align-middle';
        chapaCell.textContent = info.chapa;

        const nomeCell = document.createElement('td');
        nomeCell.className = 'text-center align-middle';
        nomeCell.textContent = info.nome;

        const secaoCell = document.createElement('td');
        secaoCell.className = 'text-center align-middle';
        secaoCell.textContent = info.secao;

        const funcaoCell = document.createElement('td');
        funcaoCell.className = 'text-center align-middle';
        funcaoCell.textContent = info.funcao;


        const blankCell = document.createElement('td');
        blankCell.className = 'text-center align-middle';
        blankCell.textContent = " ";

        let atvBtn = document.createElement("a");
        let atvImg = document.createElement("img");
        if (info.ativoLeader === 0) {

            row.classList.add("bg-danger", "bg-opacity-50");
            atvBtn.classList.add('btn', 'btn-light');
            atvImg.src = "/img/arrow-clockwise.svg";

            atvBtn.onclick = (event) => {
                event.preventDefault();
                ReactiveLeader(info.idLeader);
            };

        } else {

            atvBtn.classList.add('btn', 'btn-danger');
            atvImg.src = "/img/x-lg.svg";

            atvBtn.onclick = (event) => {
                event.preventDefault();
                DeactiveLeader(info.idLeader);
            };

            //atvBtn.onclick = function (event) {
            //    event.preventDefault();
            //    DeactiveLeader(info.idLeader);
            //};

        }
        atvBtn.appendChild(atvImg);
        blankCell.appendChild(atvBtn);


        row.appendChild(idLeaderCell);
        row.appendChild(chapaCell);
        row.appendChild(nomeCell);
        row.appendChild(secaoCell);
        row.appendChild(funcaoCell);
        row.appendChild(blankCell);
        tbody.appendChild(row);

    });

}


const loadingSearchLeader = (loadingSpan) => {

    document.querySelectorAll('.btn').forEach(btn => {
        btn.disabled = true;
    });

    document.querySelectorAll('.form-control').forEach(input => {
        input.readOnly = true;
    });

    document.getElementById(loadingSpan).classList.remove('visually-hidden');


    return () => {

        document.querySelectorAll('.btn').forEach(btn => {
            btn.disabled = false;
        });

        document.querySelectorAll('.form-control').forEach(input => {
            input.readOnly = false;
        });

        document.getElementById(loadingSpan).classList.add('visually-hidden');

    }
}


document.getElementById("addNewLeaderInput").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        const chapanomeLeader = document.getElementById("addNewLeaderInput").value;
        GetEmployeeInformation(event.target.value);
    }
});


document.getElementById('btnSearchEmployee').addEventListener('click', function () {
    console.log('clicked');
    const chapanomeLeader = document.getElementById("addNewLeaderInput").value;
    if (chapanomeLeader !== "") {
        GetEmployeeInformation(chapanomeLeader);
    }
});



/*document.getElementById('leaderInformationForm').addEventListener('submit', function (event) {*/
document.getElementById('saveButton').addEventListener('click', async function (event) {
    event.preventDefault();

    let cleanup;
    try {

        var situation = document.getElementById('situacaoLeaderLabel').textContent;
        if (situation.startsWith("D")) throw new ValidationError('Employee is already dismissed.');

        var chapa = document.getElementById('chapaLeaderLabel').textContent;
        if (!chapa) throw new ValidationError('Chapa is empty.');

        var codpessoa = document.getElementById('codPessoaMemberHidden').value;
        if (!codpessoa) throw new ValidationError('codpessoa is empty.');

        var userSib = document.getElementById('UserId').value;
        if (!userSib) throw new ValidationError('userSib is empty.');

        let form = document.getElementById('leaderInformationForm');

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
            window.location.href = `/AdminSide/Index`;
        }, 2000);




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

    //let form = document.getElementById('leaderInformationForm');

    //var situation = document.getElementById('situacaoLeaderLabel').textContent;
    //if (situation.startsWith("D")) {
    //    document.getElementById('situacaoLeaderFeedback').style.display = 'block';
    //    isValid = false;
    //    appendAlertWithoutAnimation('Employee already in status D', 'warning');
    //    return;
    //}

    //var chapa = document.getElementById('chapaLeaderLabel').textContent;
    //if (!chapa) {
    //    document.getElementById('chapaLeaderFeedback').style.display = 'block';
    //    isValid = false;
    //    appendAlertWithoutAnimation('Chapa block is empty', 'warning');
    //    return;
    //}

    //var codpessoa = document.getElementById('codPessoaMemberHidden').value;
    //if (!codpessoa) {
    //    isValid = false;
    //    appendAlertWithoutAnimation('codpessoa is empty', 'warning');
    //    return;
    //}

    //var userSib = document.getElementById('UserId').value;
    //if (!userSib) {
    //    isValid = false;
    //    appendAlertWithoutAnimation('userSib is empty', 'warning');
    //    return;
    //}

    //if (isValid) {
    //    form.action = "InsertLeader";
    //    form.submit();
    //}

});


async function GetEmployeeInformation(chapanomeLeader) {

    let cleanup;
    try {

        if (!chapanomeLeader) throw new ValidationError('Por favor insira chapa');


        let url = `/AdminSide/GetEmployeeInformation?givenInfo=${chapanomeLeader}`;

        const result = await fetchJson(url);

        populateEmployeeInformation(result);

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



    //$.ajax({
    //    url: '/AdminSide/GetEmployeeInformation',
    //    type: 'GET',
    //    data: { givenInfo: chapanomeLeader },
    //    success: function (data) {

    //        if (data.success) {
    //            if (data.memberinfo) {
    //                console.log(data.memberinfo);
    //                populateEmployeeInformation(data.memberinfo);
    //            } else if (data.totalResult) {

    //            } else {
    //                console.log(data.message);
    //                clearLeaderForm();
    //                appendAlertWithoutAnimation(data.message, 'danger');
    //            }
    //        } else {
    //            console.log(data.message);
    //            clearLeaderForm();
    //            appendAlertWithoutAnimation(data.message, 'danger');
    //        }
    //    },
    //    error: function (error) {
    //        console.error('Error fetching items:', error);
    //        appendAlertWithoutAnimation(error, 'danger');
    //    }
    //});
}


function populateEmployeeInformation(employee) {

    console.log(employee);

    if (employee.imagebase64 !== null) {
        document.getElementById("imgEmployee").src = employee.imageStringByte;
    } else {
        document.getElementById("imgEmployee").src = "/img/image-not-available.jpg";
    }

    document.getElementById("chapaLeaderLabel").innerText = employee.chapa;
    document.getElementById("chapaMemberHidden").value = employee.chapa;
    document.getElementById("codPessoaMemberHidden").value = employee.codPessoa;
    document.getElementById("terceiroMemberHidden").value = employee.idTerceiro;
    document.getElementById("terceiroMemberHidden").value = employee.idTerceiro;
    document.getElementById("UserId").value = employee.idUserSib;

    document.getElementById("nomeLeaderLabel").innerText = employee.nome;
    document.getElementById("nomeHidden").value = employee.nome;

    document.getElementById("situacaoLeaderLabel").innerText = employee.codSituacao;
    if (employee.codSituacao === "A") {
        document.getElementById("situacaoLeaderLabel").innerText = employee.codSituacao;
        document.getElementById("chapaLeaderLabel").style.color = "black";
        document.getElementById("nomeLeaderLabel").style.color = "black";
        document.getElementById("situacaoLeaderLabel").style.color = "black";
    } else {
        // Format the date to dd/mm/yyyy
        let demissaoDate = new Date(employee.dataDemissao);
        let formattedDate = demissaoDate.toLocaleDateString('pt-BR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });

        // Set the text and apply red color
        let situacaoLabel = document.getElementById("situacaoLeaderLabel");
        situacaoLabel.innerText = `${employee.codSituacao} - ${formattedDate}`;
        situacaoLabel.style.color = "red";
        document.getElementById("chapaLeaderLabel").style.color = "red";
        document.getElementById("nomeLeaderLabel").style.color = "red";
    }

    document.getElementById("funcaoLeaderLabel").innerText = employee.funcao;

    document.getElementById("secaoLeaderLabel").innerText = employee.secao;


    document.getElementById("addNewLeaderInput").value = '';

    document.getElementById('saveButton').style.display = 'block';

    //if (employee.isRegistered == true) {
    //    document.getElementById("LeaderMemRelIdHolder").value = employee.idLeadereMemRel;
    //    document.getElementById('saveButton').style.display = 'none';
    //    document.getElementById('reactivateButton').style.display = 'block';
    //} else if (employee.isRegistered == false) {
    //    document.getElementById('reactivateButton').style.display = 'none';
    //    document.getElementById('saveButton').style.display = 'block';
    //} else {
    //    document.getElementById('reactivateButton').style.display = 'none';
    //    document.getElementById('saveButton').style.display = 'none';
    //}

    /*document.getElementById('saveButton').style.display = 'block';*/
}


function clearLeaderForm() {
    // Clear labels
    document.getElementById('chapaLeaderLabel').textContent = '';
    document.getElementById('nomeLeaderLabel').textContent = '';
    document.getElementById('situacaoLeaderLabel').textContent = '';
    document.getElementById('funcaoLeaderLabel').textContent = '';
    document.getElementById('secaoLeaderLabel').textContent = '';

    // Clear hidden inputs
    document.getElementById('chapaMemberHidden').value = '';
    document.getElementById('codPessoaMemberHidden').value = '';
    document.getElementById('terceiroMemberHidden').value = '';
    document.getElementById('nomeHidden').value = '';
    document.getElementById('situacaoLeaderHidden').value = '';
    document.getElementById('funcaoLeaderHidden').value = '';
    document.getElementById('secaoLeaderHidden').value = '';

    // Reset image
    document.getElementById('imgEmployee').src = '/img/image-not-available.jpg';

    // Remove validation messages
    document.getElementById('chapaLeaderFeedback').style.display = 'none';
    document.getElementById('situacaoLeaderFeedback').style.display = 'none';
}













const DeactiveLeader = async (idLeader) => {

    let cleanup;
    try {

        let url = `/AdminSide/DeactiveLeader?id=${idLeader}`;

        const result = await fetchJson(url, {
            method: 'POST',
            headers: {
                /*'Content-Type': 'multipart/form-data',*/
                'Accept': 'application/json'
            }
        });

        if (!result) throw new ValidationError('Invalid Deactivation.');

        appendAlertWithoutAnimation('Success', 'success');

                    setTimeout(function () {
                        window.location.href = '/AdminSide/Index';
                    }, 1000);


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


    //$.ajax({
    //    url: "/AdminSide/DeactiveLeader",
    //    type: 'POST',
    //    data: { id: idLeader },
    //    success: function (response) {
    //        if (response.success) {

    //            appendAlertWithoutAnimation('Success', 'success');

    //            setTimeout(function () {
    //                window.location.href = '/AdminSide/Index';
    //            }, 1000);

    //        } else {
    //            appendAlert(response.message, 'danger');
    //        }
    //    },
    //    error: function (error) {
    //        console.error('Error fetching details:', error);
    //    }
    //});

}


const ReactiveLeader = async (idLeader) => {

    let cleanup;
    try {

        let url = `/AdminSide/ReactiveLeader?id=${idLeader}`;

        const result = await fetchJson(url, {
            method: 'POST',
            headers: {
                /*'Content-Type': 'multipart/form-data',*/
                'Accept': 'application/json'
            }
        });

        if (!result) throw new ValidationError('Invalid Deactivation.');

        appendAlertWithoutAnimation('Success', 'success');

        setTimeout(function () {
            window.location.href = '/AdminSide/Index';
        }, 1000);


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


    //$.ajax({
    //    url: "/AdminSide/ReactiveLeader",
    //    type: 'POST',
    //    data: { id: idLeader },
    //    success: function (response) {
    //        if (response.success) {

    //            appendAlertWithoutAnimation('Success', 'success');

    //            setTimeout(function () {
    //                window.location.href = '/AdminSide/Index';
    //            }, 1000);

    //        } else {
    //            appendAlert(response.message, 'danger');
    //        }
    //    },
    //    error: function (error) {
    //        console.error('Error fetching details:', error);
    //    }
    //});

}