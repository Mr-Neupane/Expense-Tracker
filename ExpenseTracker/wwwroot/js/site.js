document.querySelectorAll('.dropdown-submenu > a').forEach(function (link) {
  link.addEventListener('click', function (event) {
    event.preventDefault();
    event.stopPropagation();
    var parent = link.parentElement;
    parent.classList.toggle('show');
  });
});
