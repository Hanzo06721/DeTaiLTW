(function () {
  'use strict';

  const workspace = document.getElementById('buildPcWorkspace');
  const modal = document.getElementById('buildPcModal');
  if (!workspace || !modal) return;

  const titleEl = document.getElementById('pickerModalTitle');
  const subtitleEl = document.getElementById('pickerModalSubtitle');
  const searchInput = document.getElementById('pickerSearchInput');
  const compatOnlyCheckbox = document.getElementById('pickerCompatOnly');
  const resultCountEl = document.getElementById('pickerResultCount');
  const listEl = document.getElementById('pickerList');
  const selectForm = document.getElementById('pickerSelectForm');
  const formSlotKey = document.getElementById('pickerFormSlotKey');
  const formProductId = document.getElementById('pickerFormProductId');

  let activeSlotKey = '';
  let cachedItems = [];
  let searchTimer = null;
  let fetchAbortController = null;
  let isMutating = false;

  const formatVnd = (amount) =>
    new Intl.NumberFormat('vi-VN').format(Math.round(Number(amount) || 0)) + ' đ';

  const escapeHtml = (str) => {
    const div = document.createElement('div');
    div.textContent = str ?? '';
    return div.innerHTML;
  };

  const openModal = (slotKey, slotTitle, slotSubtitle) => {
    activeSlotKey = slotKey;
    titleEl.textContent = `Chọn ${slotTitle}`;
    subtitleEl.textContent = slotSubtitle || '';
    searchInput.value = '';
    compatOnlyCheckbox.checked = false;
    modal.classList.add('is-open');
    modal.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    loadProducts();
    setTimeout(() => searchInput.focus(), 50);
  };

  const closeModal = () => {
    modal.classList.remove('is-open');
    modal.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    if (fetchAbortController) {
      fetchAbortController.abort();
      fetchAbortController = null;
    }
  };

  const postAndUpdateWorkspace = async (url, formData) => {
    if (isMutating) return false;
    isMutating = true;
    workspace.classList.add('is-loading');

    try {
      const res = await fetch(url, {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        body: formData
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const html = await res.text();
      workspace.innerHTML = html;
      return true;
    } catch (err) {
      console.error('BuildPc AJAX error:', err);
      return false;
    } finally {
      workspace.classList.remove('is-loading');
      isMutating = false;
    }
  };

  const renderList = () => {
    const compatOnly = compatOnlyCheckbox.checked;
    const filtered = compatOnly
      ? cachedItems.filter((item) => item.compatibilityStatus !== 'error')
      : cachedItems;

    resultCountEl.textContent = `Tìm thấy ${filtered.length} linh kiện còn hàng`;

    if (filtered.length === 0) {
      listEl.innerHTML =
        '<div class="picker-empty">Không có linh kiện phù hợp hoặc nhóm này chưa có sản phẩm còn hàng trong cơ sở dữ liệu.</div>';
      return;
    }

    listEl.innerHTML = filtered
      .map((item) => {
        const p = item.product;
        const attrs = (p.attributes || [])
          .slice(0, 6)
          .map(
            (a) =>
              `<span class="spec-pill">${escapeHtml(a.name)}: <strong>${escapeHtml(a.value)}</strong></span>`
          )
          .join('');

        let compatHtml = '';
        if (item.compatibilityStatus === 'error' && item.compatibilityReason) {
          compatHtml = `<div class="picker-compat is-error">✕ ${escapeHtml(item.compatibilityReason)}</div>`;
        } else if (item.compatibilityStatus === 'warning' && item.compatibilityReason) {
          compatHtml = `<div class="picker-compat is-warning">! ${escapeHtml(item.compatibilityReason)}</div>`;
        } else if (item.compatibilityReason) {
          compatHtml = `<div class="picker-compat is-ok">✓ ${escapeHtml(item.compatibilityReason)}</div>`;
        }

        const thumbHtml = p.thumbnail
          ? `<img src="${escapeHtml(p.thumbnail)}" alt="${escapeHtml(p.name)}" onerror="this.style.display='none';this.nextElementSibling.style.display='grid';" /><span class="builder-thumb-fallback" style="display:none;">▣</span>`
          : `<span class="builder-thumb-fallback">▣</span>`;

        const btnLabel = item.isSelected ? 'Đang chọn ✓' : 'Chọn linh kiện';
        const btnClass = item.isSelected ? 'btn btn-outline' : 'btn btn-primary';

        return `
          <div class="picker-card ${item.isSelected ? 'is-current' : ''} ${item.compatibilityStatus === 'error' ? 'has-conflict' : ''}">
            <div class="picker-thumb">${thumbHtml}</div>
            <div class="picker-info">
              <div class="picker-name">${escapeHtml(p.name)}</div>
              <div class="picker-sub">
                <span>SKU: <strong>${escapeHtml(p.sku)}</strong></span>
                <span>·</span>
                <span>Danh mục: ${escapeHtml(p.categoryName || 'Linh kiện')}</span>
                <span>·</span>
                <span>Còn <strong>${p.stockQuantity}</strong> SP</span>
              </div>
              ${attrs ? `<div class="builder-spec-pills">${attrs}</div>` : ''}
              ${compatHtml}
            </div>
            <div class="picker-cta">
              <div class="picker-price">${formatVnd(p.price)}</div>
              <button type="button" class="${btnClass}" data-pick-product="${p.id}">${btnLabel}</button>
            </div>
          </div>`;
      })
      .join('');
  };

  const loadProducts = async () => {
    if (!activeSlotKey) return;
    if (fetchAbortController) fetchAbortController.abort();
    fetchAbortController = new AbortController();

    resultCountEl.textContent = 'Đang tìm kiếm...';
    listEl.innerHTML = '<div class="picker-empty">Đang tải danh sách linh kiện từ SQL Server...</div>';

    try {
      const q = encodeURIComponent(searchInput.value.trim());
      const url = `/BuildPc/Products?slot=${encodeURIComponent(activeSlotKey)}&q=${q}`;
      const res = await fetch(url, { signal: fetchAbortController.signal });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      cachedItems = await res.json();
      renderList();
    } catch (err) {
      if (err && err.name === 'AbortError') return;
      resultCountEl.textContent = 'Lỗi tải dữ liệu';
      listEl.innerHTML =
        '<div class="picker-empty picker-empty-error">Không thể tải danh sách linh kiện. Vui lòng kiểm tra kết nối SQL Server và thử lại.</div>';
    }
  };

  // Mở modal chọn linh kiện bằng event delegation (hoạt động sau mỗi lần cập nhật AJAX)
  workspace.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-open-picker]');
    if (!btn) return;
    openModal(
      btn.getAttribute('data-slot-key') || '',
      btn.getAttribute('data-slot-title') || '',
      btn.getAttribute('data-slot-subtitle') || ''
    );
  });

  // Chặn submit form (Đổi số lượng, Xóa linh kiện, Xóa cấu hình) để xử lý AJAX không reload trang
  workspace.addEventListener('submit', async (e) => {
    const form = e.target.closest('form[data-ajax-form]');
    if (!form) return;
    e.preventDefault();

    const confirmText = form.getAttribute('data-confirm');
    if (confirmText && !window.confirm(confirmText)) return;

    const formData = new FormData(form);
    if (e.submitter && e.submitter.name) {
      formData.set(e.submitter.name, e.submitter.value);
    }

    const ok = await postAndUpdateWorkspace(form.action, formData);
    if (!ok) {
      form.submit();
    }
  });

  // Cho phép nhập trực tiếp số lượng (AJAX không reload trang)
  const submitTypedQty = async (input) => {
    const form = input.closest('form[data-ajax-form]');
    if (!form) return;
    const formData = new FormData(form);
    formData.set('quantity', input.value);
    const ok = await postAndUpdateWorkspace(form.action, formData);
    if (!ok) {
      form.submit();
    }
  };

  workspace.addEventListener('change', (e) => {
    const input = e.target.closest('[data-qty-input]');
    if (!input) return;
    submitTypedQty(input);
  });

  workspace.addEventListener('keydown', (e) => {
    const input = e.target.closest('[data-qty-input]');
    if (!input || e.key !== 'Enter') return;
    e.preventDefault();
    submitTypedQty(input);
  });

  document.querySelectorAll('[data-close-picker]').forEach((btn) => {
    btn.addEventListener('click', closeModal);
  });

  modal.addEventListener('click', (e) => {
    if (e.target === modal) closeModal();
  });

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && modal.classList.contains('is-open')) {
      closeModal();
    }
  });

  searchInput.addEventListener('input', () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(loadProducts, 240);
  });

  compatOnlyCheckbox.addEventListener('change', renderList);

  // Chọn linh kiện trong modal bằng AJAX (không load lại trang)
  listEl.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-pick-product]');
    if (!btn || isMutating) return;
    const productId = btn.getAttribute('data-pick-product');
    if (!productId || !activeSlotKey) return;

    btn.disabled = true;
    btn.textContent = 'Đang chọn...';

    formSlotKey.value = activeSlotKey;
    formProductId.value = productId;
    const formData = new FormData(selectForm);

    const ok = await postAndUpdateWorkspace(selectForm.action, formData);
    if (ok) {
      closeModal();
    } else {
      selectForm.submit();
    }
  });
})();
