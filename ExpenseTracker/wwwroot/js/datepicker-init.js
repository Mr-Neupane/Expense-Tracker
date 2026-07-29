document.addEventListener("DOMContentLoaded", function () {
    $(".nepali-date-picker").each(function () {
        NepaliDateHelper.initDatePicker(this);
    });

    function getCurrentTime() {
        var now = new Date();
        return now.toLocaleTimeString(undefined, {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });
    }

    $(document).on("submit", "form", function () {
        var form = this;
        var pickers = form.querySelectorAll(".nepali-date-picker");
        var canPost = true;

        pickers.forEach(function (el) {
            var bsValue = el.value;
            if (!bsValue || bsValue.trim().length === 0) return;

            var normalized = bsValue.replace(/\//g, "-");
            if (!NepaliFunctions.BS.ValidateDate(normalized)) {
                el.classList.add("is-invalid");
                var existingMsg = el.nextElementSibling;
                if (!existingMsg || !existingMsg.classList.contains("invalid-feedback")) {
                    var msg = document.createElement("div");
                    msg.className = "invalid-feedback";
                    msg.textContent = "Date not valid";
                    el.parentNode.insertBefore(msg, el.nextSibling);
                } else {
                    existingMsg.textContent = "Date not valid";
                }
                canPost = false;
                return;
            }

            el.classList.remove("is-invalid");
            var feedback = el.nextElementSibling;
            if (feedback && feedback.classList.contains("invalid-feedback")) {
                feedback.remove();
            }

            let adDate = NepaliFunctions.BS2AD(normalized, "YYYY-MM-DD", "YYYY-MM-DD");
            let engDate = adDate + " " + getCurrentTime();

            let modelName = el.dataset.modelName;
            if (modelName) {
                el.removeAttribute("name");
                let hidden = document.createElement("input");
                hidden.type = "hidden";
                hidden.name = modelName;
                hidden.value = engDate;
                hidden.classList.add("duplicate");
                form.appendChild(hidden);
            }
        });

        if (!canPost) {
            return false;
        }
    });
});
