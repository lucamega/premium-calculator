window.addEventListener("DOMContentLoaded", (event) => {
    let insurances;
    let birthDateInput = document.getElementById("birth-date-input");
    let stateSelect = document.getElementById("state-select");
    let planSelect = document.getElementById("plan-select");
    let ageInput = document.getElementById("age-input");
    let getPremiumButton = document.getElementById("get-premium-button");
    let periodSelect = document.getElementById("period-select");
    let rowsContainer = document.getElementById("rows-container");

    birthDateInput.addEventListener("input", onBirthDateInput);
    ageInput.addEventListener("keypress", onIntegerInputPress);
    getPremiumButton.addEventListener("click", onGetPremiumClick);
    periodSelect.addEventListener("change", onPeriodChange);

    function onIntegerInputPress(event) {
        if (isNaN(String.fromCharCode(event.keyCode))) {
            event.preventDefault();
        }
    }

    function onDecimalInputKeyPress(event) {
        if (isNaN(this.value + String.fromCharCode(event.keyCode))) {
            event.preventDefault();
        }
    }

    function onBirthDateInput() {
        var birthDate = birthDateInput.valueAsDate;
        if (!birthDate) {
            ageInput.value = null;
            return;
        }

        var today = new Date();
        if (today < birthDate) {
            ageInput.value = null;
            return;
        }

        var years = today.getFullYear() - birthDate.getFullYear();
        var months = today.getMonth() - birthDate.getMonth();
        if (months < 0 ||
            (months == 0 && today.getDate() < birthDate.getDate())) {
            years--;
        }
        ageInput.value = years;
    }

    async function onGetPremiumClick() {
        periodSelect.disabled = true;
        rowsContainer.innerHTML = "";
        insurances = [];

        let birthDate = birthDateInput.valueAsDate;
        let state = stateSelect.value;
        let plan = planSelect.value;
        let age = ageInput.value;
        if (!birthDate || !state || !plan || !age) {
            alert("Please fill all the fields.");
            return;
        }

        getPremiumButton.disabled = true;

        try {
            let getPremiumResponse = await fetch("/premium", {
                method: "POST",
                body: JSON.stringify({
                    birthDate: birthDate.toISOString(),
                    state: state,
                    plan: plan,
                    age: parseInt(age),
                })
            });

            let response = await getPremiumResponse.json();

            let message = response.message;
            if (message) {
                alert(message);
                return;
            }

            for (let i = 0; i < response.length; i++) {
                let insurance = response[i];

                let carrierInput = document.createElement("input");
                carrierInput.type = "text";
                carrierInput.value = insurance.carrier;
                let carrierCell = document.createElement("div");
                carrierCell.className = "table-cell";
                carrierCell.appendChild(carrierInput);

                let premiumInput = document.createElement("input");
                premiumInput.type = "text";
                premiumInput.value = insurance.premium;
                premiumInput.addEventListener("keypress", onDecimalInputKeyPress);
                let premiumCell = document.createElement("div");
                premiumCell.className = "table-cell";
                premiumCell.appendChild(premiumInput);

                let annualInput = document.createElement("input");
                annualInput.type = "text";
                annualInput.addEventListener("keypress", onDecimalInputKeyPress);
                let annualCell = document.createElement("div");
                annualCell.className = "table-cell";
                annualCell.appendChild(annualInput);

                let monthlyInput = document.createElement("input");
                monthlyInput.type = "text";
                monthlyInput.addEventListener("keypress", onDecimalInputKeyPress);
                let monthlyCell = document.createElement("div");
                monthlyCell.className = "table-cell";
                monthlyCell.appendChild(monthlyInput);

                fillPeriodCells(insurance.premium, annualInput, monthlyInput);

                let row = document.createElement("div");
                row.className = "table-row";
                row.appendChild(carrierCell);
                row.appendChild(premiumCell);
                row.appendChild(annualCell);
                row.appendChild(monthlyCell);

                rowsContainer.appendChild(row);
                insurances.push({ premium: insurance.premium, annualInput: annualInput, monthlyInput: monthlyInput });
            }

            if (insurances.length > 0) {
                periodSelect.disabled = false;
            }
            else {
                alert("No premiums found.");
            }
        }
        catch (exception) {
            alert("Error when calculating premiums.");
            throw exception;
        }
        finally {
            getPremiumButton.disabled = false;
        }
    }

    function onPeriodChange() {
        for (let i = 0; i < insurances.length; i++) {
            let insurance = insurances[i];
            fillPeriodCells(insurance.premium, insurance.annualInput, insurance.monthlyInput);
        }
    }

    function fillPeriodCells(premium, annualInput, monthlyInput) {
        let amount = parseFloat(premium);
        switch (periodSelect.value) {
            case "M":
                annualInput.value = amount * 12;
                monthlyInput.value = amount;
                break;

            case "Q":
                annualInput.value = amount * 4;
                monthlyInput.value = amount / 3;
                break;

            case "SA":
                annualInput.value = amount * 2;
                monthlyInput.value = amount / 6;
                break;

            case "A":
                annualInput.value = amount;
                monthlyInput.value = amount / 12;
                break;
        }
    }
});
