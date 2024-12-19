//TODO:: preliezt
$(document).ready(function () {
    $('.delete-user').on('click', function () {
        const userId = $(this).data('user-id');
        if (confirm('Are you sure you want to delete this user?')) {
            $.ajax({
                url: '/Admin/Delete',
                type: 'POST',
                data: { id: userId },
                success: function (response) {
                    if (response.success) {
                        $('#user-row-' + userId).remove();
                        console.log("User deleted successfully.");
                    } else {
                        alert('Failed to delete user.');
                    }
                },
                error: function () {
                    alert('An error occurred while deleting the user.');
                }
            });
        }
    });
});
