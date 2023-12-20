// External JS File (pagination.js)

function initializeTable(tableId, searchInputId, currentPageIndicatorId, selectedUserIdsInputId, paginationContainerIdPrefix, totalPagesIndicatorId) {
    var usersPerPage = 5;
    var currentPage = 1;
    var searchText = '';
    var selectedIds = [];

    function loadUsers() {
        var startIndex = (currentPage - 1) * usersPerPage;
        var endIndex = startIndex + usersPerPage;

        // Hide all rows
        $('#' + tableId + ' tbody tr').hide();

        // Show rows for the current page and matching the search text
        $('#' + tableId + ' tbody tr').filter(function () {
            return $(this).text().toLowerCase().includes(searchText);
        }).slice(startIndex, endIndex).show();

        // Update the current page indicator
        $('#' + paginationContainerIdPrefix + ' ' + currentPageIndicatorId).text(currentPage);

        // Update the total pages indicator
        var totalPages = Math.ceil($('#' + tableId + ' tbody tr').filter(function () {
            return $(this).text().toLowerCase().includes(searchText);
        }).length / usersPerPage);

        // Check if the totalPagesIndicatorId exists and update its text
        if (totalPagesIndicatorId) {
            $('#' + paginationContainerIdPrefix + ' #' + totalPagesIndicatorId).text('Sayfa ' + currentPage + '/' + totalPages);
        }

        // Check the selected checkboxes
        selectedIds.forEach(function (id) {
            $('#' + tableId + ' #checkbox-' + id).prop('checked', true);
        });
    }

    function prevPage() {
        if (currentPage > 1) {
            currentPage--;
            loadUsers();
        }
    }

    function nextPage() {
        var totalPages = Math.ceil($('#' + tableId + ' tbody tr').filter(function () {
            return $(this).text().toLowerCase().includes(searchText);
        }).length / usersPerPage);
        if (currentPage < totalPages) {
            currentPage++;
            loadUsers();
        }
    }

    // Handle checkbox clicks
    $('#' + tableId + ' tbody').on('change', 'input[type="checkbox"]', function () {
        var checkbox = $(this);
        var id = checkbox.val();

        if (checkbox.is(":checked")) {
            selectedIds.push(id);
        } else {
            var index = selectedIds.indexOf(id);
            if (index !== -1) {
                selectedIds.splice(index, 1);
            }
        }

        // Update the hidden input field with selected user IDs
        $("#" + selectedUserIdsInputId).val(selectedIds.join(','));
    });

    // Initial load
    loadUsers();

    $('#' + searchInputId).on('input', function () {
        searchText = $(this).val().toLowerCase();
        currentPage = 1;
        loadUsers();
    });

    // Handle previous and next button clicks
    $('#' + paginationContainerIdPrefix + ' #previousPageButton').click(function () {
        event.preventDefault();
        prevPage();
    });

    $('#' + paginationContainerIdPrefix + ' #nextPageButton').click(function () {
        event.preventDefault();

        nextPage();
    });

    // Handle previous and next button clicks
    $('#' + paginationContainerIdPrefix + ' #previousPageButton2').click(function () {
        event.preventDefault();
        prevPage();
    });

    $('#' + paginationContainerIdPrefix + ' #nextPageButton2').click(function () {
        event.preventDefault();

        nextPage();
    });
}
$(document).ready(function () {
    initializeTable('userTable', 'userSearch', 'currentPage', 'selectedUserIds', 'user-pagination-container', 'userTotalPages');
    initializeTable('tournamentUserTable', 'tournamentUserSearch', 'tournamentCurrentPage', 'selectedTournamentUserIds', 'tournament-user-pagination-container', 'tournamentUserTotalPages');
    initializeTable('tournamentTable', 'tournamentSearch', 'tournamentCurrentPage', 'selectedTournamentIds', 'tournament-pagination-container', 'tournamentTotalPages');

});
