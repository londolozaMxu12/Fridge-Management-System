// Global addToCart function
function addToCart(button, fridgeId) {
    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Show loading state
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="bi bi-arrow-repeat bi-spin me-1"></i> Added';
    button.disabled = true;

    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `fridgeId=${fridgeId}&quantity=1`
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Add to cart response:', data);
            if (data.success) {
                updateCartCount();
                showToast('Success', data.message, 'success');
            } else {
                showToast('Error', data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Error', 'An error occurred while adding to cart', 'error');
        })
        .finally(() => {
            // Reset button
            button.innerHTML = originalText;
            button.disabled = false;
        });
}

// Include this file in your _Layout.cshtml
