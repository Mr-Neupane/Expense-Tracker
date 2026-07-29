let NepaliDateHelper = {
    initDatePicker: function (inputElement) {
        if (!inputElement) return;

        let existingValue = inputElement.value;
        let bsDate;
        if (existingValue && existingValue.trim().length > 0) {
            bsDate = NepaliFunctions.AD2BS(existingValue, "YYYY-MM-DD");
        } else {
            bsDate = NepaliFunctions.AD2BS(new Date(), "YYYY-MM-DD");
        }

        if (inputElement.name) {
            inputElement.dataset.modelName = inputElement.name;
            inputElement.removeAttribute("name");
        }

        inputElement.NepaliDatePicker({
            language: "english",
            dateFormat: "YYYY/MM/DD",
            animation: "slide",
            disableDaysAfter: 1,
            value: bsDate
        });
    }
};
