// Immediately-Invoked Function Expression (IIFE)
(function (window) {
    window.MyUtils = window.MyUtils || {};

    MyUtils.helpers = {
        MakeHiddenInputUni: function (name, value) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            input.value = value;
            return input;
        },
        TestUni: function () {
            return 'TEST';
        },
        ConvertToDateHelp: function (dateString) {
            const [day, month, year] = dateString.split('/');
            const expirationDate = new Date(`${year}-${month}-${day}`);

            expirationDate.setHours(0, 0, 0, 0);

            return expirationDate;
        }
    };
})(window);