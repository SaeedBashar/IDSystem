
(function() {
  "use strict";

const datatables = [...document.querySelectorAll('.datatable')]
  datatables.forEach(datatable => {
      new simpleDatatables.DataTable(datatable);
  })

})();