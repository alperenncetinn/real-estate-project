// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Small helpers for showing/hiding the shared loading and error overlays

function showLoading(message) {
	const overlay = document.getElementById('loadingOverlay');
	if (!overlay) return;
	if (message) {
		const text = overlay.querySelector('.app-loading-text');
		if (text) text.textContent = message;
	}
	overlay.style.display = 'flex';
}

function hideLoading() {
	const overlay = document.getElementById('loadingOverlay');
	if (!overlay) return;
	overlay.style.display = 'none';
}

function showError(message, title) {
	const overlay = document.getElementById('errorOverlay');
	if (!overlay) return;
	const msg = document.getElementById('errorMessage');
	if (msg) msg.textContent = message || 'Bilinmeyen bir hata. Lütfen tekrar deneyin.';
	const t = overlay.querySelector('.app-error-title');
	if (t && title) t.textContent = title;
	overlay.style.display = 'flex';
}

function hideError() {
	const overlay = document.getElementById('errorOverlay');
	if (!overlay) return;
	overlay.style.display = 'none';
}

// Export to window for inline usage
window.showLoading = showLoading;
window.hideLoading = hideLoading;
window.showError = showError;
window.hideError = hideError;
