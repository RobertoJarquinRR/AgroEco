document.addEventListener('DOMContentLoaded', () => {
    const navButtons = document.querySelectorAll('.sub-nav .nav-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    navButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetId = button.dataset.target;

            navButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(tab => tab.classList.remove('active'));

            button.classList.add('active');
            const targetTab = document.getElementById(targetId);
            if (targetTab) {
                targetTab.classList.add('active');
            }
        });
    });

    const accordions = document.querySelectorAll('.accordion-item');
    accordions.forEach(accordion => {
        const header = accordion.querySelector('.accordion-header');
        header.addEventListener('click', () => {
            accordions.forEach(item => {
                if (item !== accordion) {
                    item.classList.remove('open');
                    item.classList.add('closed');
                    const arrow = item.querySelector('.accordion-arrow');
                    if (arrow) {
                        arrow.classList.remove('fa-chevron-up');
                        arrow.classList.add('fa-chevron-down');
                    }
                }
            });

            accordion.classList.toggle('open');
            accordion.classList.toggle('closed');
            const arrow = accordion.querySelector('.accordion-arrow');
            if (arrow) {
                if (accordion.classList.contains('open')) {
                    arrow.classList.remove('fa-chevron-down');
                    arrow.classList.add('fa-chevron-up');
                } else {
                    arrow.classList.remove('fa-chevron-up');
                    arrow.classList.add('fa-chevron-down');
                }
            }
        });
    });
});