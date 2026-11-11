// Counts down to the date in the data-date attribute of #countdown
const countdown = document.getElementById('countdown');
if (countdown) {
    const target = new Date(countdown.dataset.date).getTime();

    function tick() {
        const left = Math.max(0, target - Date.now());
        document.getElementById('cd-days').textContent = Math.floor(left / 86400000);
        document.getElementById('cd-hours').textContent = Math.floor((left % 86400000) / 3600000);
        document.getElementById('cd-minutes').textContent = Math.floor((left % 3600000) / 60000);
        document.getElementById('cd-seconds').textContent = Math.floor((left % 60000) / 1000);
    }

    tick();
    setInterval(tick, 1000);
}
