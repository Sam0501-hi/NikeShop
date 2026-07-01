// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Hàm cập nhật số lượng giỏ hàng trên badge
function updateCartBadge() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('cart-badge');
            if (badge) {
                badge.innerText = data.count;
            }
        })
        .catch(err => console.error("Lỗi cập nhật giỏ hàng: ", err));
}

// Chạy hàm ngay khi trang web tải xong
document.addEventListener('DOMContentLoaded', updateCartBadge);