// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// site.js

var logoutTimer;

const appendAlert = (message, type) => {
    const alertPlaceholder = document.getElementById('liveAlertPlaceholder');
    if (!alertPlaceholder) {
        console.error('Alert placeholder not found');
        return;
    }

    const wrapper = document.createElement('div');
    wrapper.innerHTML = [
        `<div class="alert alert-${type} alert-dismissible" role="alert">`,
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        '</div>'
    ].join('');

    alertPlaceholder.append(wrapper);

    // Automatically close the alert after 3 seconds with fade-out effect
    setTimeout(() => {
        wrapper.classList.add('fade-out');
        setTimeout(() => {
            wrapper.remove();
        }, 500); // Match this duration with the fadeOut animation duration
    }, 3000);
};

const appendAlertWithoutAnimation = (message, type) => {
    const alertPlaceholder = document.getElementById('liveAlertPlaceholder');
    if (!alertPlaceholder) {
        console.error('Alert placeholder not found');
        return;
    }

    const wrapper = document.createElement('div');
    wrapper.innerHTML = [
        `<div class="alert alert-${type} alert-dismissible" role="alert">`,
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        '</div>'
    ].join('');

    alertPlaceholder.append(wrapper);
};


function startLogoutTimer() {
    logoutTimer = setTimeout(function () {
        // Show a warning message
        alert("Your session has expired, please login again");

        // Redirect to logout action or perform logout logic
        window.location.href = "/Home/Logout";
    },  20 * 60 * 1000); // Show warning after 15 minutes of inactivity (adjust as needed)
}

function resetLogoutTimer() {
    clearTimeout(logoutTimer); // Reset the logout timer
    startLogoutTimer(); // Start the timer again
}

// Attach event listeners to reset the timer on user activity
$(document).on("mousemove keypress", function () {
    resetLogoutTimer();
});

// Start the logout timer on page load
$(document).ready(function () {
    startLogoutTimer();
});




var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popoverdd"]'))
var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl)
})

tinymce.init({
    selector: '#mytextarea',
    toolbar: 'undo redo | styles | bold italic | alignleft aligncenter alignright alignjustify | outdent indent',
    menubar: ''
    //plugins: 'anchor autolink charmap codesample emoticons image link lists media searchreplace table visualblocks wordcount checklist mediaembed casechange export formatpainter pageembed linkchecker a11ychecker tinymcespellchecker permanentpen powerpaste advtable advcode editimage advtemplate ai mentions tinycomments tableofcontents footnotes mergetags autocorrect typography inlinecss',
    //toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image media table mergetags | addcomment showcomments | spellcheckdialog a11ycheck typography | align lineheight | checklist numlist bullist indent outdent | emoticons charmap | removeformat',
    //tinycomments_mode: 'embedded',
    //tinycomments_author: 'Author name',
    //mergetags_list: [
    //    { value: 'First.Name', title: 'First Name' },
    //    { value: 'Email', title: 'Email' },
    //],
    //ai_request: (request, respondWith) => respondWith.string(() => Promise.reject("See docs to implement AI Assistant")),
});

$(document).ready(function () {
    $(".loadingButton").on("click", function () {
        // Add the 'show' class to display the loader
        $(".loadingIndicator").addClass("show");
        $("#container").addClass("blurred");
        //// You may want to perform your search logic or submit the form here
        //// For demonstration purposes, let's simulate a delay using setTimeout
        //setTimeout(function () {
        //    // Remove the 'show' class to hide the loader after a delay (simulating search completion)
        //    $(".loadingIndicator").removeClass("show");
        //}, 2000); // Adjust the delay as needed
    });
});

function ddlCatalogoChanged(selectElement) {
    var selectedValue = selectElement.value;

    $("#ddlClasse").empty();
    $("#ddlTipo").empty();

    $.ajax({
        type: "GET",
        url: "/Catalogoes/LoadClasse",
        data: { selectedValue: selectedValue },
        success: function (data) {
            $("#ddlClasse").empty().append($('<option>', {
                value: '', // Set an empty value for the default option
                text: 'Selecionar...'
            }));
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

function ddlClasseChanged(selectElement) {
    var selectedValue = selectElement.value;
    console.log("Selected value:", selectedValue);

    $.ajax({
        type: "GET",
        url: "/Catalogoes/LoadTipo",
        data: { selectedValue: selectedValue },
        success: function (data) {
            $("#ddlTipo").empty().append($('<option>', {
                value: '', // Set an empty value for the default option
                text: 'Selecionar...'
            }));
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

function SomenteNumeros(campo, evt) {
    //00.00.00.00000
    var xPos = PosicaoCursor(campo);
    evt = getEvent(evt);
    var tecla = getKeyCode(evt);
    if (!teclaValida(tecla))
        return;

    vr = campo.value = filtraNumeros(filtraCampo(campo));

    MovimentaCursor(campo, xPos);
}

function formataCodigoProduto(campo, evt) {
    //00.00.00.00000
    var xPos = PosicaoCursor(campo);
    evt = getEvent(evt);
    var tecla = getKeyCode(evt);
    if (!teclaValida(tecla))
        return;


    vr = campo.value = filtraNumeros(filtraCampo(campo));
    tam = vr.length;


    if (tam >= 2 && tam < 4)
        campo.value = vr.substr(0, 2) + '.' + vr.substr(2);
    else if (tam >= 4 && tam < 6)
        campo.value = vr.substr(0, 2) + '.' + vr.substr(2, 2) + '.' + vr.substr(4);
    else if (tam >= 6 && tam < 8)
        campo.value = vr.substr(0, 2) + '.' + vr.substr(2, 2) + '.' + vr.substr(4, 2) + '.' + vr.substr(6);
    else if (tam >= 8 && tam < 12)
        campo.value = vr.substr(0, 2) + '.' + vr.substr(2, 2) + '.' + vr.substr(4, 2) + '.' + vr.substr(6, 5);

    MovimentaCursor(campo, xPos);
}

function PosicaoCursor(textarea) {
    var pos = 0;
    if (typeof textarea.selectionStart != 'undefined') {
        // For modern browsers
        pos = textarea.selectionStart;
    } else if (document.selection) {
        // For IE < 9
        textarea.focus();
        var range = document.selection.createRange();
        var rangeDuplicate = range.duplicate();
        rangeDuplicate.moveToElementText(textarea);
        rangeDuplicate.setEndPoint('EndToEnd', range);
        pos = rangeDuplicate.text.length - range.text.length;
    }

    // Adjust cursor position if it's on a dot
    var value = textarea.value;
    if (value[pos] === '.') {
        // Move cursor one position to the right
        pos++;
    }

    console.log("Cursor position:", pos); // Debugging statement

    return pos;
}





//function PosicaoCursor(textarea) {
//    var pos = 0;
//    if (typeof (document.selection) != 'undefined') {
//        //IE
//        var range = document.selection.createRange();
//        var i = 0;
//        for (i = textarea.value.length; i > 0; i--) {
//            if (range.moveStart('character', 1) == 0)
//                break;
//        }
//        pos = i;
//    }
//    if (typeof (textarea.selectionStart) != 'undefined') {
//        //FireFox
//        pos = textarea.selectionStart;
//    }

//    if (pos == textarea.value.length)
//        return 0; //retorna 0 quando não precisa posicionar o elemento
//    else
//        return pos; //posição do cursor
//}

function getEvent(evt) {
    if (!evt) evt = window.event; //IE
    return evt;
}

function teclaValida(tecla) {
    if (tecla == 8 //backspace
        //Esta evitando o post, quando são pressionadas estas teclas.
        //Foi comentado pois, se for utilizado o evento texchange, é necessario o post.
        || tecla == 9 //TAB
        || tecla == 27 //ESC
        || tecla == 16 //Shif TAB
        || tecla == 45 //insert
        || tecla == 46 //delete
        || tecla == 35 //home
        || tecla == 36 //end
        || tecla == 37 //esquerda
        || tecla == 38 //cima
        || tecla == 39 //direita
        || tecla == 40)//baixo
        return false;
    else
        return true;
}

function filtraNumeros(campo) {
    var s = "";
    var cp = "";
    vr = campo;
    tam = vr.length;
    for (i = 0; i < tam; i++) {
        if (vr.substring(i, i + 1) == "0" ||
            vr.substring(i, i + 1) == "1" ||
            vr.substring(i, i + 1) == "2" ||
            vr.substring(i, i + 1) == "3" ||
            vr.substring(i, i + 1) == "4" ||
            vr.substring(i, i + 1) == "5" ||
            vr.substring(i, i + 1) == "6" ||
            vr.substring(i, i + 1) == "7" ||
            vr.substring(i, i + 1) == "8" ||
            vr.substring(i, i + 1) == "9") {
            s = s + vr.substring(i, i + 1);
        }
    }
    return s;
    //return campo.value.replace("/", "").replace("-", "").replace(".", "").replace(",", "")
}

function filtraCampo(campo) {
    var s = "";
    var cp = "";
    vr = campo.value;
    tam = vr.length;
    for (i = 0; i < tam; i++) {
        if (vr.substring(i, i + 1) != "/"
            && vr.substring(i, i + 1) != "-"
            && vr.substring(i, i + 1) != "."
            && vr.substring(i, i + 1) != "("
            && vr.substring(i, i + 1) != ")"
            && vr.substring(i, i + 1) != ":"
            && vr.substring(i, i + 1) != ",") {
            s = s + vr.substring(i, i + 1);
        }
    }
    return s;
    //return campo.value.replace("/", "").replace("-", "").replace(".", "").replace(",", "")
}

function MovimentaCursor(textarea, pos) {
    if (pos <= 0)
        return; //se a posição for 0 não reposiciona

    if (typeof (document.selection) != 'undefined') {
        //IE
        var oRange = textarea.createTextRange();
        var LENGTH = 1;
        var STARTINDEX = pos;

        oRange.moveStart("character", -textarea.value.length);
        oRange.moveEnd("character", -textarea.value.length);
        oRange.moveStart("character", pos);
        //oRange.moveEnd("character", pos);
        oRange.select();
        textarea.focus();
    }
    if (typeof (textarea.selectionStart) != 'undefined') {
        //FireFox
        textarea.selectionStart = pos;
        textarea.selectionEnd = pos;
    }
}

function getKeyCode(evt) {
    var code;
    if (typeof (evt.keyCode) == 'number')
        code = evt.keyCode;
    else if (typeof (evt.which) == 'number')
        code = evt.which;
    else if (typeof (evt.charCode) == 'number')
        code = evt.charCode;
    else
        return 0;

    return code;
}


function toggleTheme() {
    const body = document.querySelector("body");
    const themeToggle = document.querySelector("#theme-toggle");
    const themeLabel = document.querySelector("#theme-label");
    const tables = document.querySelectorAll(".table"); //by class table
    const cells = document.querySelectorAll('.cell-style');
    const closeButtonModal = document.querySelectorAll('.ModalClose');
    const dynamicLabels = document.querySelectorAll(".dark-light-label");
    const table = document.querySelector("#myTable"); // Select your table // by Id Table No advisable to use
    const table2 = document.querySelector("#myTable2"); // Select your table
    const table3 = document.querySelector("#myTable3"); // Select your table
    const table4 = document.querySelector("#myTable4"); // Select your table
    const table5 = document.querySelector("#myTable5"); // Select your table
/*    const tables = document.querySelectorAll(".light-dark-table");*/

    if (themeToggle.checked) {
        body.dataset.theme = "dark";
        if (table) table.classList.add('table-dark');
        if (table2) table2.classList.add('table-dark');
        if (table3) table3.classList.add('table-dark');
        if (table4) table4.classList.add('table-dark');
        if (table5) table5.classList.add('table-dark');
        themeLabel.textContent = "Dark";
        themeLabel.style.color = "black";
        tables.forEach(table => {
            table.classList.add('table-dark');
        });

        dynamicLabels.forEach(dynamicLabels => {
            dynamicLabels.style.backgroundColor = "#87CEEB";
            dynamicLabels.style.color = "#000000"; 
        });

        closeButtonModal.forEach(closeButtonModal => {
            closeButtonModal.style.backgroundColor = "#87CEEB";
            closeButtonModal.style.color = "#000000";
        });

        cells.forEach(cell => {
            cell.classList.remove('cell-light-mode');
            cell.classList.add('cell-dark-mode');
        });

        //table.classList.add('table-dark');
        //table2.classList.add('table-dark');
        //table3.classList.add('table-dark');
        localStorage.setItem("theme", "dark"); // Store theme preference
    } else {
        body.dataset.theme = "light";
        if (table) table.classList.remove('table-dark');
        if (table2) table2.classList.remove('table-dark');
        if (table3) table3.classList.remove('table-dark');
        if (table4) table4.classList.remove('table-dark');
        if (table5) table5.classList.remove('table-dark');
        tables.forEach(table => {
            table.classList.remove('table-dark');
        });

        dynamicLabels.forEach(dynamicLabels => {
            dynamicLabels.style.backgroundColor = "#87CEEB";
            dynamicLabels.style.color = "#FFFFFF";
        });

        closeButtonModal.forEach(closeButtonModal => {
            closeButtonModal.style.backgroundColor = "#87CEEB";
            closeButtonModal.style.color = "#FFFFFF";
        });

        cells.forEach(cell => {
            cell.classList.remove('cell-dark-mode');
            cell.classList.add('cell-light-mode');
        });

        themeLabel.textContent = "Light";
        localStorage.setItem("theme", "light"); // Store theme preference
        /*localStorage.setItem("theme", themePreference);*/
    }
}

// Function to load theme preference when the page loads
function loadThemePreference() {
    const themePreference = localStorage.getItem("theme");

    if (themePreference === "dark") {
        document.querySelector("#theme-toggle").checked = true;
    } else {
        document.querySelector("#theme-toggle").checked = false;
    }

    // Trigger the toggleTheme function to apply the correct theme
    toggleTheme();
}

// Load theme preference when the page loads
window.addEventListener("load", loadThemePreference);

// Add an event listener to the theme-toggle checkbox
document.querySelector("#theme-toggle").addEventListener("change", toggleTheme);


document.addEventListener("DOMContentLoaded", function () {
    var submenuLinks = document.querySelectorAll('.dropdown-submenu a.dropdown-toggle');

    submenuLinks.forEach(function (link) {
        link.addEventListener("click", function (e) {
            var submenu = this.nextElementSibling;
            if (submenu.style.display === "block") {
                submenu.style.display = "none";
            } else {
                submenu.style.display = "block";
            }
            e.stopPropagation();
            e.preventDefault();
        });
    });
});