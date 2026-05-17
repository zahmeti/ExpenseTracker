function setProgress(circleId, percent) {
    const circle = document.getElementById(circleId);
    if (!circle) return;

    const radius = circle.r.baseVal.value;
    const circumference = 2 * Math.PI * radius;
    circle.style.strokeDasharray = `${circumference}`;
    const offset = circumference - percent / 100 * circumference;
    circle.style.strokeDashoffset = offset;

    // Update percentage text
    const textId = circleId.replace("Circle", "Text");
    const textElement = document.getElementById(textId);
    if (textElement) {
        textElement.textContent = `${Math.round(percent)}%`;
    }

    // Dynamic color thresholds
    if (percent < 70) {
        circle.style.stroke = "#43e97b"; // green
    } else if (percent < 90) {
        circle.style.stroke = "#f7971e"; // orange
    } else {
        circle.style.stroke = "#ff4e50"; // red
    }
}
