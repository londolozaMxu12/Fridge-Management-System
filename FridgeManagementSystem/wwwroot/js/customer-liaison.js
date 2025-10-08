// Customer Liaison-specific JavaScript functionality

class CustomerLiaisonApp {
    constructor() {
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadInitialData();
        this.initializeComponents();
    }

    setupEventListeners() {
        // Handle customer actions
        document.addEventListener('click', this.handleCustomerActions.bind(this));
        
        // Handle allocation actions
        document.addEventListener('click', this.handleAllocationActions.bind(this));
        
        // Handle inventory actions
        document.addEventListener('click', this.handleInventoryActions.bind(this));
        
        // Handle form submissions
        document.addEventListener('submit', this.handleFormSubmissions.bind(this));
    }

    initializeComponents() {
        // Initialize Select2 dropdowns
        this.initializeSelect2();
        
        // Initialize tooltips
        this.initializeTooltips();
        
        // Initialize real-time updates
        this.startRealTimeUpdates();
    }

    initializeSelect2() {
        $('select[data-select2]').select2({
            theme: 'bootstrap-5',
            width: '100%',
            placeholder: 'Select an option',
            allowClear: true
        });
    }

    initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    async loadInitialData() {
        try {
            await this.loadSystemStats();
            await this.loadPendingTasks();
        } catch (error) {
            console.error('Failed to load initial data:', error);
        }
    }

    async loadSystemStats() {
        // This would load additional system statistics
        // Implementation depends on your backend API
    }

    async loadPendingTasks() {
        // Load pending allocations, requests, etc.
        // Implementation depends on your backend API
    }

    startRealTimeUpdates() {
        // Set up real-time updates for notifications and stats
        setInterval(() => {
            this.loadNotifications();
            this.loadQuickStats();
        }, 30000); // Update every 30 seconds
    }

    handleCustomerActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const customerId = target.dataset.customerId;

        switch (action) {
            case 'view-customer':
                this.viewCustomer(customerId);
                break;
            case 'edit-customer':
                this.editCustomer(customerId);
                break;
            case 'allocate-fridge':
                this.allocateFridgeToCustomer(customerId);
                break;
            case 'view-history':
                this.viewCustomerHistory(customerId);
                break;
        }
    }

    handleAllocationActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const allocationId = target.dataset.allocationId;

        switch (action) {
            case 'view-allocation':
                this.viewAllocation(allocationId);
                break;
            case 'edit-allocation':
                this.editAllocation(allocationId);
                break;
            case 'cancel-allocation':
                this.cancelAllocation(allocationId);
                break;
            case 'complete-allocation':
                this.completeAllocation(allocationId);
                break;
        }
    }

    handleInventoryActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const itemId = target.dataset.itemId;

        switch (action) {
            case 'view-stock':
                this.viewStockDetails(itemId);
                break;
            case 'adjust-stock':
                this.adjustStock(itemId);
                break;
            case 'create-request':
                this.createPurchaseRequest(itemId);
                break;
        }
    }

    handleFormSubmissions(event) {
        const form = event.target;
        
        if (form.classList.contains('ajax-form')) {
            event.preventDefault();
            this.handleAjaxFormSubmit(form);
        }
    }

    async handleAjaxFormSubmit(form) {
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        
        try {
            // Show loading state
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
            }

            const response = await fetch(form.action, {
                method: form.method,
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const result = await response.json();
                showAlert(result.message || 'Operation completed successfully!', 'success');
                
                // Refresh data if needed
                if (result.refresh) {
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                }
            } else {
                throw new Error('Form submission failed');
            }
        } catch (error) {
            console.error('Form submission error:', error);
            showAlert('Failed to submit form. Please try again.', 'danger');
        } finally {
            // Reset button state
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = submitBtn.dataset.originalText || 'Submit';
            }
        }
    }

    // Customer management methods
    viewCustomer(customerId) {
        window.location.href = `/CustomerLiaison/Customers/Details/${customerId}`;
    }

    editCustomer(customerId) {
        window.location.href = `/CustomerLiaison/Customers/Edit/${customerId}`;
    }

    allocateFridgeToCustomer(customerId) {
        window.location.href = `/CustomerLiaison/FridgeAllocations/Create?customerId=${customerId}`;
    }

    viewCustomerHistory(customerId) {
        window.location.href = `/CustomerLiaison/Customers/History/${customerId}`;
    }

    // Allocation management methods
    viewAllocation(allocationId) {
        window.location.href = `/CustomerLiaison/FridgeAllocations/Details/${allocationId}`;
    }

    editAllocation(allocationId) {
        window.location.href = `/CustomerLiaison/FridgeAllocations/Edit/${allocationId}`;
    }

    async cancelAllocation(allocationId) {
        if (confirm('Are you sure you want to cancel this allocation?')) {
            try {
                const response = await fetch(`/CustomerLiaison/FridgeAllocations/Cancel/${allocationId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (response.ok) {
                    showAlert('Allocation cancelled successfully!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to cancel allocation');
                }
            } catch (error) {
                console.error('Error cancelling allocation:', error);
                showAlert('Failed to cancel allocation. Please try again.', 'danger');
            }
        }
    }

    async completeAllocation(allocationId) {
        try {
            const response = await fetch(`/CustomerLiaison/FridgeAllocations/Complete/${allocationId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                showAlert('Allocation marked as completed!', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            } else {
                throw new Error('Failed to complete allocation');
            }
        } catch (error) {
            console.error('Error completing allocation:', error);
            showAlert('Failed to complete allocation. Please try again.', 'danger');
        }
    }

    // Inventory management methods
    viewStockDetails(itemId) {
        window.location.href = `/CustomerLiaison/Inventory/Details/${itemId}`;
    }

    adjustStock(itemId) {
        window.location.href = `/CustomerLiaison/Inventory/Adjust/${itemId}`;
    }

    createPurchaseRequest(itemId) {
        window.location.href = `/CustomerLiaison/PurchaseRequests/Create?itemId=${itemId}`;
    }

    // Utility methods
    formatDate(dateString) {
        const options = { 
            year: 'numeric', 
            month: 'short', 
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString('en-ZA', options);
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('en-ZA', {
            style: 'currency',
            currency: 'ZAR'
        }).format(amount);
    }
}

// Initialize the customer liaison app when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.customerLiaisonApp = new CustomerLiaisonApp();
});

// Global functions for notifications
async function loadNotifications() {
    try {
        const response = await fetch('/CustomerLiaison/Notifications/GetUnread');
        if (response.ok) {
            const notifications = await response.json();
            updateNotificationDisplay(notifications);
        }
    } catch (error) {
        console.error('Failed to load notifications:', error);
    }
}

function updateNotificationDisplay(notifications) {
    const notificationCount = document.getElementById('notificationCount');
    const notificationList = document.getElementById('notificationList');
    
    if (notificationCount) {
        notificationCount.textContent = notifications.length || '0';
    }
    
    if (notificationList && notifications.length > 0) {
        let notificationHTML = '';
        notifications.forEach(notification => {
            notificationHTML += `
                <div class="notification-item ${notification.isRead ? '' : 'unread'}">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">
                            <h6 class="mb-1">${notification.title}</h6>
                            <p class="mb-1 small">${notification.message}</p>
                            <small class="notification-time">${formatTimeAgo(notification.createdAt)}</small>
                        </div>
                        ${!notification.isRead ? '<span class="badge bg-primary ms-2">New</span>' : ''}
                    </div>
                </div>
            `;
        });
        notificationList.innerHTML = notificationHTML;
    }
}

function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffHours < 24) return `${diffHours} hr ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    
    return date.toLocaleDateString();
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CustomerLiaisonApp;
}