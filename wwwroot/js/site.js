let budgetChart; // keep reference
let expenseChart; // keep reference

// Monthly Expenses Chart (Year vs Month view)
window.renderBudgetChart = (labels, data) => {
    const ctx = document.getElementById('monthlyChart');
    if (!ctx) return;

    // Destroy old chart if exists
    if (budgetChart) {
        budgetChart.destroy();
    }

    // Decide chart type based on labels length
    const chartType = labels.length <= 12 ? 'bar' : 'line';

    budgetChart = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: chartType === 'bar' ? 'Monthly Expenses' : 'Daily Expenses',
                data: data,
                backgroundColor: chartType === 'bar'
                    ? 'rgba(54, 162, 235, 0.6)'
                    : 'rgba(54, 162, 235, 0.3)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 2,
                fill: chartType === 'line'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { beginAtZero: true }
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.label;
                            let value = context.parsed.y;
                            return `${label}: €${value}`;
                        }
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                intersect: true
            },
            elements: {
                bar: {
                    borderWidth: 1,
                    borderSkipped: false,
                    hoverBorderWidth: 2
                },
                line: {
                    tension: 0.3
                },
                point: {
                    radius: 4,
                    hoverRadius: 6
                }
            },
            animation: {
                duration: 1200,
                easing: 'easeOutQuart'
            }
        }
    });
};

// Category Distribution Chart
window.renderExpenseChart = (labels, data) => {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;

    if (expenseChart) {
        expenseChart.destroy();
    }

    expenseChart = new Chart(ctx, {
        type: 'doughnut', // ✅ donut style
        data: {
            labels: labels,
            datasets: [{
                label: 'Expenses by Category',
                data: data,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.6)',   // Food
                    'rgba(54, 162, 235, 0.6)',   // Transport
                    'rgba(255, 206, 86, 0.6)',   // Shopping
                    'rgba(75, 192, 192, 0.6)',   // Entertainment
                    'rgba(153, 102, 255, 0.6)'   // Bills & Utilities
                ],
                hoverOffset: 12
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right', // ✅ move legend to right side
                    labels: {
                        generateLabels: function(chart) {
                            const dataset = chart.data.datasets[0];
                            const total = dataset.data.reduce((a, b) => a + b, 0);

                            return chart.data.labels.map((label, i) => {
                                const value = dataset.data[i];
                                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return {
                                    text: `${label}: €${value} (${percentage}%)`,
                                    fillStyle: dataset.backgroundColor[i],
                                    strokeStyle: dataset.backgroundColor[i],
                                    hidden: isNaN(value) || value === 0,
                                    index: i
                                };
                            });
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.label || '';
                            let value = context.parsed || 0;
                            let total = context.chart._metasets[0].total;
                            let percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `${label}: €${value} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                duration: 1200,
                easing: 'easeOutQuart'
            }
        }
    });
};

// ✅ Logout Toast Notification
window.showLogoutToast = () => {
    const toast = document.createElement("div");
    toast.innerText = "✅ You’ve been logged out successfully!";
    toast.style.position = "fixed";
    toast.style.bottom = "20px";
    toast.style.left = "20px";
    toast.style.background = "rgba(0,0,0,0.85)";
    toast.style.color = "#fff";
    toast.style.padding = "10px 20px";
    toast.style.borderRadius = "8px";
    toast.style.zIndex = "9999";
    toast.style.fontSize = "14px";
    toast.style.boxShadow = "0 0 10px rgba(0,0,0,0.5)";
    toast.style.transition = "opacity 0.5s ease";

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 500);
    }, 2500); // disappears after 2.5 seconds
};

window.showLoginErrorToast = () => {
    const toast = document.createElement("div");
    toast.innerText = "❌ Invalid email or password!";
    toast.style.position = "fixed";
    toast.style.bottom = "20px";
    toast.style.left = "20px";
    toast.style.background = "rgba(200,0,0,0.85)";
    toast.style.color = "#fff";
    toast.style.padding = "10px 20px";
    toast.style.borderRadius = "8px";
    toast.style.zIndex = "9999";
    toast.style.fontSize = "14px";
    toast.style.boxShadow = "0 0 10px rgba(0,0,0,0.5)";
    toast.style.transition = "opacity 0.5s ease";

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 500);
    }, 2500);
};

window.showBudgetAlertToast = (message) => {
    const toast = document.createElement("div");
    toast.innerText = `⚠️ ${message}`;
    toast.style.position = "fixed";
    toast.style.top = "20px";
    toast.style.right = "20px";
    toast.style.maxWidth = "420px";
    toast.style.background = "linear-gradient(135deg, #ff4d5f, #ff8a4c)";
    toast.style.color = "#fff";
    toast.style.padding = "14px 18px";
    toast.style.borderRadius = "10px";
    toast.style.zIndex = "9999";
    toast.style.fontSize = "14px";
    toast.style.fontWeight = "700";
    toast.style.boxShadow = "0 16px 36px rgba(255,77,95,0.3)";
    toast.style.transition = "opacity 0.5s ease, transform 0.5s ease";

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transform = "translateY(-8px)";
        setTimeout(() => toast.remove(), 500);
    }, 5000);
};
