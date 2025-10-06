// Customer-specific JavaScript functionality

class CustomerApp {
    constructor() {
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadCustomerData();
    }

    setupEventListeners() {
        // Handle fridge status updates
        document.addEventListener('click', this.handleFridgeActions.bind(this));

        // Handle fault reporting
        document.addEventListener('submit', this.handleFaultReport.bind(this));

        // Handle maintenance scheduling
        document.addEventListener('submit', this.handleMaintenanceSchedule.bind(this));
    }

    async loadCustomerData() {
        try {
            // Load customer-specific data like fridge count, pending requests, etc.
            const response = await fetch('/Customer/Dashboard/GetCustomerStats');
            if (response.ok) {
                const data = await response.json();
                this.updateDashboard(data);
            }
        } catch (error) {
            console.error('Failed to load customer data:', error);
        }
    }

    updateDashboard(data) {
        // Update quick stats on dashboard
        const elements = {
            totalFridges: document.getElementById('totalFridges'),
            operationalFridges: document.getElementById('operationalFridges'),
            pendingRequests: document.getElementById('pendingRequests'),
            activeFaults: document.getElementById('activeFaults')
        };

        for (const [key, element] of Object.entries(elements)) {
            if (element && data[key] !== undefined) {
                element.textContent = data[key];
            }
        }
    }

    handleFridgeActions(event) {
        const target = event.target;

        if (target.classList.contains('report-fault-btn')) {
            this.showFaultReportModal(target.dataset.fridgeId);
        }

        if (target.classList.contains('view-maintenance-btn')) {
            this.showMaintenanceHistory(target.dataset.fridgeId);
        }

        if (target.classList.contains('request-replacement-btn')) {
            this.showReplacementRequest(target.dataset.fridgeId);
        }
    }

    showFaultReportModal(fridgeId) {
        // This would typically open a modal for fault reporting
        const modal = new bootstrap.Modal(document.getElementById('faultReportModal'));
        document.getElementById('faultFridgeId').value = fridgeId;
        modal.show();
    }

    async handleFaultReport(event) {
        if (event.target.id === 'faultReportForm') {
            event.preventDefault();

            const formData = new FormData(event.target);
            const fridgeId = formData.get('FridgeId');
            const description = formData.get('Description');
            const urgency = formData.get('Urgency');

            try {
                const response = await fetch('/Customer/FaultReports/Create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        fridgeId: fridgeId,
                        description: description,
                        urgency: urgency
                    })
                });

                if (response.ok) {
                    showAlert('Fault reported successfully! Our team will contact you soon.', 'success');
                    bootstrap.Modal.getInstance(document.getElementById('faultReportModal')).hide();
                    event.target.reset();
                } else {
                    throw new Error('Failed to submit fault report');
                }
            } catch (error) {
                console.error('Error reporting fault:', error);
                showAlert('Failed to report fault. Please try again.', 'danger');
            }
        }
    }

    async handleMaintenanceSchedule(event) {
        if (event.target.id === 'maintenanceScheduleForm') {
            event.preventDefault();

            // Similar implementation for maintenance scheduling
            // This would handle the maintenance scheduling form submission
        }
    }

    // Utility function to format dates
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

    // Utility function to handle API errors
    handleApiError(error) {
        console.error('API Error:', error);
        showAlert('An error occurred. Please try again.', 'danger');
    }
}

// Initialize the customer app when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    window.customerApp = new CustomerApp();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CustomerApp;
}