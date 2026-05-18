document.addEventListener("DOMContentLoaded", function () {
  var dateInput = document.getElementById("date");
  var dateToInput = document.getElementById("dateto");

  if (dateInput) {
    var engDateInput = document.getElementById("engdate");
    NepaliDateHelper.initDatePicker(dateInput, engDateInput);
  }

  if (dateToInput) {
    var dateToEngInput = document.getElementById("datetoengdate");
    NepaliDateHelper.initDatePicker(dateToInput, dateToEngInput);
  }
});
