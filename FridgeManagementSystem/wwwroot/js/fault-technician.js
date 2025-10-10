// Fault Technician-specific JavaScript functionality

class FaultTechnicianApp {
    constructor() {
        this.currentRepair = null;
        this.timeTracker = null;
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadInitialData();
        this.initializeComponents();
    }

    setupEventListeners() {
        // Handle fault actions
        document.addEventListener('click', this.handleFaultActions.bind(this));

        // Handle repair actions
        document.addEventListener('click', this.handleRepairActions.bind(this));

        // Handle fridge request actions
        document.addEventListener('click', this.handleFridgeRequestActions.bind(this));

        // Handle form submissions
        document.addEventListener('submit', this.handleFormSubmissions.bind(this));

        // Handle time tracking
        document.addEventListener('click', this.handleTimeTracking.bind(this));
    }

    initializeComponents() {
        // Initialize Select2 dropdowns
        this.initializeSelect2();

        // Initialize tooltips
        this.initializeTooltips();

        // Initialize calendar if on calendar page
        this.initializeCalendar();

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

    initializeCalendar() {
        const calendarEl = document.getElementById('technicianCalendar');
        if (calendarEl) {
            this.calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek,timeGridDay'
                },
                events: '/FaultTechnician/Repairs/GetCalendarEvents',
                eventClick: this.handleCalendarEventClick.bind(this),
                eventColor: '#ffc107',
                businessHours: {
                    daysOfWeek: [1, 2, 3, 4, 5],
                    startTime: '08:00',
                    endTime: '17:00',
                }
            });
            this.calendar.render();
        }
    }

    async loadInitialData() {
        try {
            await this.loadAssignedFaults();
            await this.loadTodaySchedule();
            await this.loadPendingRequests();
        } catch (error) {
            console.error('Failed to load initial data:', error);
        }
    }

    async loadAssignedFaults() {
        // Load faults assigned to the current technician
        // Implementation depends on your backend API
    }

    async loadTodaySchedule() {
        // Load today's repair schedule
        // Implementation depends on your backend API
    }

    async loadPendingRequests() {
        // Load pending fridge requests
        // Implementation depends on your backend API
    }

    startRealTimeUpdates() {
        // Set up real-time updates for new faults and schedule changes
        setInterval(() => {
            this.loadAssignedFaults();
            this.loadTodaySchedule();
            this.loadPendingRequests();
        }, 30000); // Update every 30 seconds
    }

    // FAULT MANAGEMENT - View faults, Process fault
    handleFaultActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const faultId = target.dataset.faultId;

        switch (action) {
            case 'view-fault':
                this.viewFaultDetails(faultId);
                break;
            case 'assign-to-me':
                this.assignFaultToMe(faultId);
                break;
            case 'start-diagnosis':
                this.startDiagnosis(faultId);
                break;
            case 'process-fault':
                this.processFault(faultId);
                break;
            case 'prioritize-fault':
                this.prioritizeFault(faultId);
                break;
        }
    }

    viewFaultDetails(faultId) {
        window.location.href = `/FaultTechnician/Faults/Details/${faultId}`;
    }

    async assignFaultToMe(faultId) {
        try {
            const response = await fetch(`/FaultTechnician/Faults/AssignToMe/${faultId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                showAlert('Fault assigned to you successfully!', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            } else {
                throw new Error('Failed to assign fault');
            }
        } catch (error) {
            console.error('Error assigning fault:', error);
            showAlert('Failed to assign fault. Please try again.', 'danger');
        }
    }

    startDiagnosis(faultId) {
        window.location.href = `/FaultTechnician/Faults/Diagnose/${faultId}`;
    }

    async processFault(faultId) {
        // Process fault - update status, add notes, etc.
        const diagnosis = prompt('Enter diagnosis notes:');
        if (diagnosis) {
            try {
                const response = await fetch(`/FaultTechnician/Faults/Process/${faultId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ diagnosis: diagnosis })
                });

                if (response.ok) {
                    showAlert('Fault processed successfully!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to process fault');
                }
            } catch (error) {
                console.error('Error processing fault:', error);
                showAlert('Failed to process fault. Please try again.', 'danger');
            }
        }
    }

    async prioritizeFault(faultId) {
        const priority = prompt('Set priority (1-5, where 5 is highest):');
        if (priority && priority >= 1 && priority <= 5) {
            try {
                const response = await fetch(`/FaultTechnician/Faults/Prioritize/${faultId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ priority: parseInt(priority) })
                });

                if (response.ok) {
                    showAlert('Fault priority updated!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to update priority');
                }
            } catch (error) {
                console.error('Error updating priority:', error);
                showAlert('Failed to update priority. Please try again.', 'danger');
            }
        }
    }

    // REPAIR MANAGEMENT - Repair fridge
    handleRepairActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const repairId = target.dataset.repairId;

        switch (action) {
            case 'view-repair':
                this.viewRepairDetails(repairId);
                break;
            case 'start-repair':
                this.startRepair(repairId);
                break;
            case 'schedule-repair':
                this.scheduleRepair(repairId);
                break;
            case 'update-repair-progress':
                this.updateRepairProgress(repairId);
                break;
            case 'complete-repair':
                this.completeRepair(repairId);
                break;
            case 'notify-customer':
                this.notifyCustomer(repairId);
                break;
        }
    }

    viewRepairDetails(repairId) {
        window.location.href = `/FaultTechnician/Repairs/Details/${repairId}`;
    }

    startRepair(repairId) {
        this.currentRepair = repairId;
        this.startTimeTracker(repairId);
        window.location.href = `/FaultTechnician/Repairs/Start/${repairId}`;
    }

    scheduleRepair(repairId) {
        window.location.href = `/FaultTechnician/Repairs/Schedule/${repairId}`;
    }

    async updateRepairProgress(repairId) {
        const progress = prompt('Enter repair progress update:');
        if (progress) {
            try {
                const response = await fetch(`/FaultTechnician/Repairs/UpdateProgress/${repairId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ progress: progress })
                });

                if (response.ok) {
                    showAlert('Repair progress updated!', 'success');
                } else {
                    throw new Error('Failed to update progress');
                }
            } catch (error) {
                console.error('Error updating progress:', error);
                showAlert('Failed to update progress. Please try again.', 'danger');
            }
        }
    }

    async completeRepair(repairId) {
        const notes = prompt('Enter repair completion notes:');
        if (notes !== null) {
            try {
                const response = await fetch(`/FaultTechnician/Repairs/Complete/${repairId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        completionNotes: notes,
                        completionTime: new Date().toISOString()
                    })
                });

                if (response.ok) {
                    this.stopTimeTracker(repairId);
                    showAlert('Repair completed successfully!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to complete repair');
                }
            } catch (error) {
                console.error('Error completing repair:', error);
                showAlert('Failed to complete repair. Please try again.', 'danger');
            }
        }
    }

    async notifyCustomer(repairId) {
        const message = prompt('Enter notification message for customer:');
        if (message) {
            try {
                const response = await fetch(`/FaultTechnician/Communication/NotifyCustomer/${repairId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ message: message })
                });

                if (response.ok) {
                    showAlert('Customer notified successfully!', 'success');
                } else {
                    throw new Error('Failed to notify customer');
                }
            } catch (error) {
                console.error('Error notifying customer:', error);
                showAlert('Failed to notify customer. Please try again.', 'danger');
            }
        }
    }

    // FRIDGE CONDITION MANAGEMENT - Update fridge condition
    handleFridgeRequestActions(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const requestId = target.dataset.requestId;
        const fridgeId = target.dataset.fridgeId;

        switch (action) {
            case 'view-request':
                this.viewFridgeRequest(requestId);
                break;
            case 'process-request':
                this.processFridgeRequest(requestId);
                break;
            case 'update-condition':
                this.updateFridgeCondition(fridgeId);
                break;
            case 'mark-repairable':
                this.markFridgeRepairable(fridgeId);
                break;
            case 'mark-unrepairable':
                this.markFridgeUnrepairable(fridgeId);
                break;
        }
    }

    viewFridgeRequest(requestId) {
        window.location.href = `/FaultTechnician/FridgeRequests/Details/${requestId}`;
    }

    async processFridgeRequest(requestId) {
        const action = prompt('Process request (approve/deny):');
        if (action && (action.toLowerCase() === 'approve' || action.toLowerCase() === 'deny')) {
            try {
                const response = await fetch(`/FaultTechnician/FridgeRequests/Process/${requestId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ action: action.toLowerCase() })
                });

                if (response.ok) {
                    showAlert(`Fridge request ${action.toLowerCase()}d successfully!`, 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to process request');
                }
            } catch (error) {
                console.error('Error processing request:', error);
                showAlert('Failed to process request. Please try again.', 'danger');
            }
        }
    }

    async updateFridgeCondition(fridgeId) {
        const condition = prompt('Enter fridge condition (Excellent/Good/Fair/Poor/Critical):');
        const notes = prompt('Enter condition notes:');

        if (condition) {
            try {
                const response = await fetch(`/FaultTechnician/Fridges/UpdateCondition/${fridgeId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        condition: condition,
                        notes: notes
                    })
                });

                if (response.ok) {
                    showAlert('Fridge condition updated successfully!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error('Failed to update condition');
                }
            } catch (error) {
                console.error('Error updating condition:', error);
                showAlert('Failed to update condition. Please try again.', 'danger');
            }
        }
    }

    async markFridgeRepairable(fridgeId) {
        try {
            const response = await fetch(`/FaultTechnician/Fridges/MarkRepairable/${fridgeId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                showAlert('Fridge marked as repairable!', 'success');
            } else {
                throw new Error('Failed to mark fridge as repairable');
            }
        } catch (error) {
            console.error('Error marking fridge:', error);
            showAlert('Failed to mark fridge. Please try again.', 'danger');
        }
    }

    async markFridgeUnrepairable(fridgeId) {
        const reason = prompt('Enter reason for unrepairable status:');
        if (reason) {
            try {
                const response = await fetch(`/FaultTechnician/Fridges/MarkUnrepairable/${fridgeId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ reason: reason })
                });

                if (response.ok) {
                    showAlert('Fridge marked as unrepairable!', 'warning');
                } else {
                    throw new Error('Failed to mark fridge as unrepairable');
                }
            } catch (error) {
                console.error('Error marking fridge:', error);
                showAlert('Failed to mark fridge. Please try again.', 'danger');
            }
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

    // Time tracking methods
    handleTimeTracking(event) {
        const target = event.target.closest('[data-action]');
        if (!target) return;

        const action = target.dataset.action;
        const repairId = target.dataset.repairId;

        switch (action) {
            case 'start-timer':
                this.startTimeTracker(repairId);
                break;
            case 'pause-timer':
                this.pauseTimeTracker();
                break;
            case 'stop-timer':
                this.stopTimeTracker(repairId);
                break;
        }
    }

    startTimeTracker(repairId) {
        this.timeTracker = {
            repairId: repairId,
            startTime: new Date(),
            isRunning: true
        };

        // Update UI to show timer is running
        document.querySelectorAll('[data-repair-id="' + repairId + '"] [data-action="start-timer"]')
            .forEach(btn => {
                btn.disabled = true;
                btn.innerHTML = '<i class="bi bi-pause-fill me-1"></i>Timer Running';
            });

        showAlert('Time tracker started for repair #' + repairId, 'info');
    }

    pauseTimeTracker() {
        if (this.timeTracker && this.timeTracker.isRunning) {
            this.timeTracker.isRunning = false;
            // Update UI
            showAlert('Time tracker paused', 'warning');
        }
    }

    async stopTimeTracker(repairId) {
        if (this.timeTracker && this.timeTracker.repairId === repairId) {
            const endTime = new Date();
            const duration = endTime - this.timeTracker.startTime;

            try {
                const response = await fetch(`/FaultTechnician/Repairs/LogTime/${repairId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        duration: duration,
                        startTime: this.timeTracker.startTime.toISOString(),
                        endTime: endTime.toISOString()
                    })
                });

                if (response.ok) {
                    showAlert('Time logged successfully!', 'success');
                    this.timeTracker = null;
                } else {
                    throw new Error('Failed to log time');
                }
            } catch (error) {
                console.error('Error logging time:', error);
                showAlert('Failed to log time. Please try again.', 'danger');
            }
        }
    }

    handleCalendarEventClick(info) {
        const repairId = info.event.extendedProps.repairId;
        if (repairId) {
            this.viewRepairDetails(repairId);
        }
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

    formatDuration(milliseconds) {
        const hours = Math.floor(milliseconds / 3600000);
        const minutes = Math.floor((milliseconds % 3600000) / 60000);
        return `${hours}h ${minutes}m`;
    }
}

// Initialize the fault technician app when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    window.faultTechnicianApp = new FaultTechnicianApp();
});

// Global functions for notifications
async function loadNotifications() {
    try {
        const response = await fetch('/FaultTechnician/Notifications/GetUnread');
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
            const urgencyClass = notification.isUrgent ? 'urgent' : '';
            const unreadClass = !notification.isRead ? 'unread' : '';

            notificationHTML += `
                <div class="technician-notification ${urgencyClass} ${unreadClass}">
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
    module.exports = FaultTechnicianApp;
}