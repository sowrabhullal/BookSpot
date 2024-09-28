var dt;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dt = $('#tbldata').DataTable({
        "ajax": {
            "url": '/admin/company/getall',
            "type": "GET",
            "datatype": "json",
            "error": function (xhr, error, code) {
                console.error("Error fetching data: ", error);
            }
        },
        "columns": [
            { data: 'name', "width": "10%" },
            { data: 'streetAddress', "width": "20%" },
            { data: 'city', "width": "10%" },
            { data: 'state', "width": "10%" },
            { data: 'postalCode', "width": "10%" },
            { data: 'phoneNumber', "width": "15%" },
            {
                data: 'id',
                "width": "30%",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/admin/company/upsert?id=${data}" class="btn btn-primary btn-sm mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick=deletecompany('/admin/company/delete/${data}') class="btn btn-danger btn-sm mx-2" 
                                <i class="bi bi-trash"></i> Delete
                            </a>
                        </div>  
                    `;
                }
            }
        ],
        "processing": true,
        "serverSide": true,
        "responsive": true
    });
}

function deletecompany(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dt.ajax.reload();
                    toastr.success(data.message)
                }
            })
        }
    });
}
