console.log("Fragment 1 JS loaded. Lodash available:", typeof _ !== 'undefined');
document.addEventListener('DOMContentLoaded', () => {
    const el = document.getElementById('frag1-content');
    if(el) el.style.borderColor = 'darkblue';
});
