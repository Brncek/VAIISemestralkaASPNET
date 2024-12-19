//TODO:: preliezt
$(document).ready(function () {
    $("#lookupButton").click(function () {
        const vin = $("#vinInput").val();

        if (!vin) {
            alert("Please enter a VIN.");
            return;
        }

        $.ajax({
            url: "/Garage/GetVinDetails",
            type: "GET",
            data: { vin: vin },
            success: function (data) {
                if (data.success) {
                    const details = data.details;
                    const container = $("#vinDetails dl");
                    container.empty();

                    // Populate details dynamically
                    Object.entries(details).forEach(([key, value]) => {
                        container.append(`
                            <dt class="col-sm-3">${key}</dt>
                            <dd class="col-sm-9">${value}</dd>
                        `);
                    });

                    $("#vinDetails").show();
                } else {
                    alert("Failed to fetch details: " + data.message);
                }
            },
            error: function () {
                alert("An error occurred while fetching VIN details.");
            }
        });
    });
});
