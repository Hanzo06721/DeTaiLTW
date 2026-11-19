// Chỉ xử lý hiển thị menu. Chưa có giỏ hàng, đăng nhập, SQL hoặc nghiệp vụ.
(function () {
  'use strict';
  const mobileButton = document.querySelector('[data-menu-toggle]');
  const menu = document.getElementById('primary-nav');
  if (mobileButton && menu) {
    mobileButton.addEventListener('click', function () {
      const open = menu.classList.toggle('is-open');
      mobileButton.setAttribute('aria-expanded', String(open));
    });
  }
  const adminButton = document.querySelector('[data-admin-toggle]');
  const sidebar = document.getElementById('admin-sidebar');
  const overlay = document.querySelector('[data-admin-overlay]');
  if (adminButton && sidebar && overlay) {
    const setOpen = (open) => {
      sidebar.classList.toggle('is-open', open);
      overlay.classList.toggle('is-open', open);
      adminButton.setAttribute('aria-expanded', String(open));
    };
    adminButton.addEventListener('click', () => setOpen(!sidebar.classList.contains('is-open')));
    overlay.addEventListener('click', () => setOpen(false));
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') setOpen(false); });
  }
})();
