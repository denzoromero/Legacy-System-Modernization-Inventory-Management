//console.log(ItemList);

//console.log(ItemList.items);

if (ItemList && Array.isArray(ItemList.items) && ItemList.items.some(item => item.intStatus === 0)) {
    console.log("Status 0 item exists.");
    /*document.getElementById('ProcessSubmitBtn').disabled = false;*/
    /*document.getElementById('ProcessSubmitBtn').style.display = 'block';*/
    document.getElementById('ProcessSubmitBtn').classList.remove('visually-hidden');
} else {
    console.log("Status 0 item doesnt exists.");
    document.getElementById('ProcessSubmitBtn').classList.add('visually-hidden');
    document.getElementById('checkAllCheckboxes').classList.add('visually-hidden');
    /*document.getElementById('ProcessSubmitBtn').style.display = 'none';*/
    /*document.getElementById('checkAllCheckboxes').style.display = 'none';*/
}



function toggleCheckboxes(source) {
    // Get all checkboxes with the class 'itemCheckbox'
    const checkboxes = document.querySelectorAll('.itemCheckbox');

    // Loop through each checkbox and set its checked state to match the "Check All" checkbox
    checkboxes.forEach(checkbox => {
        checkbox.checked = source.checked;
    });
}

function goBackToIndex() {
    /*window.location.href = '@Url.Action("Index", "CLVM")';*/

    window.location.href = `/PrepareReservation/Index`;
}

document.getElementById('formPrepareAction').addEventListener('submit', function () {

    const btn = document.getElementById('ProcessSubmitBtn');

    btn.disabled = true;

    btn.querySelector('#btnImage').classList.add('visually-hidden');
    btn.querySelector('#loadingConfirmation').classList.remove('visually-hidden');

});

//document.getElementById('checkAllCheckboxes').addEventListener('change', function () {

//    const checkboxes = document.querySelectorAll('.itemCheckbox');
//    checkboxes.forEach(checkbox => {
//        checkbox.checked = this.checked;
//    });

//});