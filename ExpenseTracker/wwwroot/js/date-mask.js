function applyDateMask(inputElement) {
    if (!inputElement) return;

    inputElement.addEventListener("input", function () {
        let v = inputElement.value.replace(/[^0-9]/g, "");

        if (v.length > 4) v = v.slice(0, 4) + "-" + v.slice(4);
        if (v.length > 7) v = v.slice(0, 7) + "-" + v.slice(7);

        inputElement.value = v.slice(0, 10);
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const dateInputs = document.querySelectorAll("[data-date-mask]");
    dateInputs.forEach(input => applyDateMask(input));
});
