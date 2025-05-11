
const modalOriginalData = {};

function saveModalData(modalId) {
  const modal = document.getElementById(modalId);
  modalOriginalData[modalId] = {};

  modal.querySelectorAll('.modal-field').forEach(field => {
    if (field.type === 'checkbox') {
      modalOriginalData[modalId][field.name] = field.checked;
    } else {
      modalOriginalData[modalId][field.name] = field.value;
    }
  });

  // Handle image previews (e.g. <img class="modal-image" name="previewImage">)
  modal.querySelectorAll('.modal-image').forEach(img => {
    modalOriginalData[modalId][img.name] = img.src;
  });

  // Optional: Save innerHTML of partials (use with caution if needed)
  modal.querySelectorAll('.modal-partial').forEach(partial => {
    modalOriginalData[modalId][partial.id] = partial.innerHTML;
  });
}

function restoreModalData(modalId) {
  const modal = document.getElementById(modalId);

  modal.querySelectorAll('.modal-field').forEach(field => {
    if (field.type === 'checkbox') {
      if (modalOriginalData[modalId]?.[field.name] !== undefined) {
        field.checked = modalOriginalData[modalId][field.name];
      }
    } else {
      if (modalOriginalData[modalId]?.[field.name] !== undefined) {
        field.value = modalOriginalData[modalId][field.name];
      }
    }
  });

  modal.querySelectorAll('.modal-image').forEach(img => {
    if (modalOriginalData[modalId]?.[img.name] !== undefined) {
      img.src = modalOriginalData[modalId][img.name];
    }
  });

  modal.querySelectorAll('.modal-partial').forEach(partial => {
    if (modalOriginalData[modalId]?.[partial.id] !== undefined) {
      partial.innerHTML = modalOriginalData[modalId][partial.id];
    }
  });
}

// Auto-bind to all modals with data-modal attribute
document.querySelectorAll('[data-modal]').forEach(modal => {
  modal.addEventListener('shown.bs.modal', () => {
    saveModalData(modal.id);
  });
});

// Restore on cancel button click
document.querySelectorAll('.cancel-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    const modalId = btn.dataset.target;
    restoreModalData(modalId);
  });
});
