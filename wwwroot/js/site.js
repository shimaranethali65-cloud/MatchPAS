// BlindMatchPAS — confirmation modal + helpers
(function () {
    'use strict';

    const modalEl = document.getElementById('bmConfirmModal');
    if (!modalEl || typeof bootstrap === 'undefined') return;

    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const titleEl = document.getElementById('bmConfirmModalTitle');
    const bodyEl = document.getElementById('bmConfirmModalBody');
    const btnEl = document.getElementById('bmConfirmModalBtn');
    const iconWrap = document.getElementById('bmConfirmModalIcon');
    let pendingForm = null;

    function setButtonVariant(variant) {
        btnEl.className = 'btn px-4 ' + (variant === 'danger' ? 'btn-danger' : 'btn-bm-primary');
    }

    function setIcon(variant) {
        if (!iconWrap) return;
        iconWrap.className = 'rounded-circle d-inline-flex p-2 align-items-center justify-content-center ' +
            (variant === 'danger' ? 'bg-danger bg-opacity-10 text-danger' : 'bg-primary bg-opacity-10 text-primary');
        const i = iconWrap.querySelector('i');
        if (i) {
            i.className = variant === 'danger' ? 'bi bi-exclamation-triangle-fill' : 'bi bi-question-circle';
        }
    }

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;
        if (!form.hasAttribute('data-bm-confirm')) return;

        if (form.dataset.bmConfirmed === '1') {
            delete form.dataset.bmConfirmed;
            return;
        }

        e.preventDefault();
        pendingForm = form;

        const title = form.getAttribute('data-bm-title') || 'Please confirm';
        const message = form.getAttribute('data-bm-message') || 'Do you want to continue?';
        const variant = (form.getAttribute('data-bm-variant') || 'primary').toLowerCase();

        if (titleEl) titleEl.textContent = title;
        if (bodyEl) bodyEl.textContent = message;
        setButtonVariant(variant === 'danger' ? 'danger' : 'primary');
        setIcon(variant === 'danger' ? 'danger' : 'primary');

        modal.show();
    });

    btnEl.addEventListener('click', function () {
        if (pendingForm) {
            pendingForm.dataset.bmConfirmed = '1';
            if (typeof pendingForm.requestSubmit === 'function') {
                pendingForm.requestSubmit();
            } else {
                pendingForm.submit();
            }
            pendingForm = null;
        }
        modal.hide();
    });

    modalEl.addEventListener('hidden.bs.modal', function () {
        pendingForm = null;
    });
})();
