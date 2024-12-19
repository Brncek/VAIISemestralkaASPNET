//TODO:: preliezt
$(".delete-image").click(function () {
    var button = $(this);
    var folder = button.data("folder");
    var fileName = button.data("file");

    $.ajax({
        url: "/Services/DeleteImage",
        type: "POST",
        data: { folder: folder, fileName: fileName },
        success: function (response) {
            if (response.success) {
                button.closest("div").remove(); 
            } else {
                alert("Failed to delete image: " + response.message);
            }
        },
        error: function () {
            alert("An error occurred while deleting the image.");
        }
    });
});

$("#uploadButton").click(function () {
    var files = $("#fileInput")[0].files;
    var folder = $("#folderName").val();

    if (files.length === 0) {
        alert("Please select files to upload.");
        return;
    }

    var formData = new FormData();
    formData.append("folder", folder);
    for (var i = 0; i < files.length; i++) {
        formData.append("files", files[i]);
    }

    $.ajax({
        url: "/Services/UploadImages",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                response.files.forEach(function (fileUrl) {
                    var fileName = fileUrl.split('/').pop();
                    $("#image-container").append(`
                        <div class="col-md-3 text-center mb-3">
                            <img src="${fileUrl}" class="img-thumbnail" />
                            <button type="button" class="btn btn-danger btn-sm mt-2 delete-image"
                                    data-folder="${folder}" data-file="${fileName}">Delete</button>
                        </div>
                    `);
                });
            } else {
                alert("Failed to upload images: " + response.message);
            }
        },
        error: function () {
            alert("An error occurred while uploading the images.");
        }
    });
});
