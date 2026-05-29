var NepaliDateHelper = {
  initDatePicker: function (inputElement, hiddenEngInput) {
    if (!inputElement) return;

    var defaultBsDate = NepaliFunctions.AD2BS(new Date(), "YYYY-MM-DD", "YYYY/MM/DD");

    inputElement.NepaliDatePicker({
      language: "english",
      dateFormat: "YYYY/MM/DD",
      animation: "slide",
      disableDaysAfter: 1,
      value: defaultBsDate
    });

    inputElement.addEventListener("blur", function () {
      var bsDate = inputElement.value;
      if (bsDate) {
        var adDate = NepaliFunctions.BS2AD(bsDate, "YYYY/MM/DD", "YYYY-MM-DD");
        if (hiddenEngInput) {
          hiddenEngInput.value = adDate == null ? new Date().toISOString().slice(0, 10) : adDate;
        }
      }
    });

    if (hiddenEngInput) {
      hiddenEngInput.value = new Date().toISOString().slice(0, 10);
    }
  },

  initDateRange: function (fromInput, fromEngInput, toInput, toEngInput) {
    this.initDatePicker(fromInput, fromEngInput);
    this.initDatePicker(toInput, toEngInput);
  }
};
