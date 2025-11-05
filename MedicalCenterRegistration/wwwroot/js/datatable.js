$(document).ready(function () {

    const SSconfig = window.dataTableSSConfig; // server-side rendering config


    new DataTable('#table', {
        scrollX: true,
        order: [[0, 'desc']],
        language: {
            url: window.localeURL,
            paginate: {
                previous: '‹',
                next: '›',
                last: '»',
                first: '«'
            }
        },
        // serverSide rendering settings
        ...(!!SSconfig ? {
            serverSide: true,
            ajax: {
                url: SSconfig.ajaxUrl,
                type: 'POST'
            },
            columns: SSconfig.columns,
        }: {})
    });
});
