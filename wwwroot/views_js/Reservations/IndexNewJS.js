
/*console.log('working');*/


document.getElementById("chaveInput").addEventListener("keydown", async function (event) {
    if (event.key === "Enter") {
        /*   const chapanomeLeader = document.getElementById("addNewLeaderInput").value;*/

        this.disabled = true;
        document.getElementById('loadingConfirmation').classList.remove('visually-hidden');

        try {

            const url = `/PrepareReservation/getItemDetail?id=${this.value}`;

            const checkReservation = await fetchJson(url);

            window.location.href = `/PrepareReservation/ProcessListReservation?id=${checkReservation}`;

        } catch (error) {

            console.error(error);
            if (error.status === 401 || error.status === 403) {
                handleAuthenticationError(error);
            } else {
                appendAlertWithoutAnimation(error.message, 'danger');
            }

            this.disabled = false;
            document.getElementById('loadingConfirmation').classList.add('visually-hidden');

        }

        //const chapanomeLeader = event.target.value;
        //console.log(chapanomeLeader);
        //getReservedItem(chapanomeLeader);    
    }
});

//function getReservedItem(itemId) {
//    $.ajax({
//        url: '/PrepareReservation/getItemDetail',
//        type: 'GET',
//        data: { id: itemId },
//        success: function (data) {
//            if (data.success) {

//                /*window.location.href = `/PrepareReservation/ProcessItem?id=${itemId}`;*/
//                window.location.href = `/PrepareReservation/ProcessListReservation?id=${data.id}`;

//            } else {
//                appendAlert(data.message, 'danger');
//            }
//        },
//        error: function (error) {
//            console.error('Error fetching items:', error);
//            appendAlertWithoutAnimation(error, 'danger');
//        }
//    });
//}

document.getElementById("controlSelect").addEventListener("change", async function () {

    try {

        const url = `/PrepareReservation/getControlList?status=${this.value}`;

        const controlList = await fetchJson(url);

        if (controlList.length > 0) {
            makeControlListTable(controlList);
        } else {
            const tbody = document.getElementById("controlListTable").querySelector("tbody");
            tbody.innerHTML = '';
            throw new Error(`Nenhum resultado encontrado para ${this.options[this.selectedIndex].text}`);
        }

    } catch (error) {

        console.error(error);
        if (error.status === 401 || error.status === 403) {
            handleAuthenticationError(error);
        } else {
            appendAlertWithoutAnimation(error.message, 'danger');
        }

    }



    //$.ajax({
    //    url: '/PrepareReservation/getControlList',
    //    type: 'GET',
    //    data: { status: event.target.value },
    //    success: function (data) {

    //        if (data.success) {
    //            makeControlListTable(data.groupReservation);
    //        } else {
    //            const tbody = document.getElementById("controlListTable").querySelector("tbody");
    //            tbody.innerHTML = '';
    //            appendAlert(data.message, 'danger');
    //        }

    //    },
    //    error: function (error) {
    //        console.error('Error fetching items:', error);
    //        appendAlertWithoutAnimation(error, 'danger');
    //    }
    //});

});

function makeControlListTable(controllist) {
    /*console.log(controllist);*/
    const tbody = document.getElementById("controlListTable").querySelector("tbody");
    tbody.innerHTML = '';

    controllist.forEach((item, index) => {

        const row = document.createElement('tr');

        if (item.actualStatus === "Expired") {
            row.className = 'table-danger';
        }

        const controlCell = document.createElement('td');
        controlCell.className = 'text-center align-middle';
        controlCell.textContent = item.controlId;

        const itemsCell = document.createElement('td');
        itemsCell.className = 'text-center align-middle';
        itemsCell.textContent = item.leaderName;

        const dataRegistroCell = document.createElement('td');
        dataRegistroCell.className = 'text-center align-middle';
        dataRegistroCell.textContent = item.controlDataRegistroString;

        const viewCell = document.createElement('td');
        viewCell.className = 'text-center align-middle';

        const viewLink = document.createElement('a');
        //if (item.groupStatus == 1) {
        //    viewLink.href = `/PrepareReservation/ProcessListReservation/?id=${item.controlId}`; 
        //} else {
        //    viewLink.href = `/PrepareReservation/PrepareItems/?id=${item.controlId}`; 
        //}

        viewLink.href = `/PrepareReservation/PrepareItems/?id=${item.controlId}`; 
        viewLink.textContent = 'View';
        viewLink.className = 'btn btn-link';

        viewCell.appendChild(viewLink);

        row.appendChild(controlCell);
        row.appendChild(itemsCell);
        row.appendChild(dataRegistroCell);
        row.appendChild(viewCell);
        tbody.appendChild(row);

    });

}