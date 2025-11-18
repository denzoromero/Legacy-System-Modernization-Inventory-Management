
//document.getElementById("CrachaInput").addEventListener('copy', e => e.preventDefault());
//document.getElementById("CrachaInput").addEventListener('cut', e => e.preventDefault());
//document.getElementById("CrachaInput").addEventListener('paste', e => e.preventDefault());

console.log('validation', openTermsValidation);

if (openTermsValidation === true) {
    console.log('working here');

    document.addEventListener('DOMContentLoaded', function () {
        var modal = document.getElementById('termsModal');
        var modalInstance = new bootstrap.Modal(modal);
        modalInstance.show();
    });

}


document.addEventListener('DOMContentLoaded', function () {

    let inputElement = document.getElementById('CrachaInput');
    inputElement.value = '';

    //inputElement.addEventListener('copy', e => e.preventDefault());
    //inputElement.addEventListener('cut', e => e.preventDefault());
    //inputElement.addEventListener('paste', e => e.preventDefault());

    inputElement.focus();

});

let rfidTimeout;

document.getElementById('CrachaInput').addEventListener('input', (event) => {
    /*console.log(event.target.value.trim());*/
    //if (event.target.value.trim()) {
    //    document.getElementById('loadingConfirmation').classList.remove('visually-hidden');
    //    document.getElementById('btnSearch').disabled = true;
    //    event.target.readOnly = true;      

    //    document.getElementById('formEmployeeCracha').submit();


    //}

    clearTimeout(rfidTimeout);

    rfidTimeout = setTimeout(() => {

        document.getElementById('loadingConfirmation').classList.remove('visually-hidden');
        document.getElementById('btnSearch').disabled = true;
        event.target.readOnly = true;

        document.getElementById('formEmployeeCracha').submit();

    }, 150);



});

window.addEventListener('beforeunload', () => {
    clearTimeout(rfidTimeout);
});


document.getElementById('formEmployeeCracha').addEventListener('submit', function (event) {

    event.preventDefault();
    var form = event.target;
    var isValid = true;

    var crachaInput = document.getElementById('CrachaInput').value;
    /*console.log(crachaInput.trim());*/
    if (!crachaInput.trim()) {
        isValid = false;
        appendAlertWithoutAnimation('No Cracha is inputted', 'warning');
        event.preventDefault();
    }

    if (isValid) {
        form.submit();
    }

});


document.getElementById('TermCracha').addEventListener('input', async (event) => {

    let cleanup 

    clearTimeout(rfidTimeout);

    rfidTimeout = setTimeout(async () => {

        try
        {

            if (!event.target.value.trim()) {
                throw new Error('Por favor, insira o número do crachá.');
            }

            const modalElement = document.getElementById('termsModal');
            const closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"], .btn-close');

            const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);

            cleanup = loadingForTermsCondition(modal, closeButtons);

            const url = `/HandoutReservation/AddTermsControl?icard=${event.target.value.trim()}`;

            const result = await fetchJson(url, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!result.success) {
                throw new Error(result.message);
            }

            appendAlert(result.message, 'success');

            setTimeout(function () {
                window.location.href = `/HandoutReservation/CompleteReservationPage?icard=${event.target.value.trim()}`;
            }, 1000);


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


const loadingForTermsCondition = (modal, closeButtons) => {

    const modalElement = modal._element;

    const preventHide = (e) => {
        e.preventDefault();
        return false;
    };

    modalElement.addEventListener('hide.bs.modal', preventHide);
    closeButtons.forEach(btn => btn.style.pointerEvents = 'none');

    document.getElementById('loadingTermsCondition').classList.remove('visually-hidden');

    document.getElementById('TermCracha').readOnly = true;


    return () => {

        modalElement.removeEventListener('hide.bs.modal', preventHide);
        closeButtons.forEach(btn => btn.style.pointerEvents = 'auto');

        document.getElementById('loadingTermsCondition').classList.add('visually-hidden');

        document.getElementById('TermCracha').readOnly = false;

    }
}



//function getReservedItem(itemId) {

//    $.ajax({
//        url: '/PrepareReservation/getItemDetail',
//        type: 'GET',
//        data: { key: itemId },
//        success: function (data) {
//            if (data.success) {

//                /*window.location.href = `/PrepareReservation/ProcessItem?id=${itemId}`;*/
//                window.location.href = `/PrepareReservation/ProcessItem?id=${data.itemdetail.idReservation}`;

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

