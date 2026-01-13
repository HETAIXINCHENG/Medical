document.addEventListener('DOMContentLoaded', () => {
    const containers = document.querySelectorAll('.bottom-nav-container');
    if (!containers.length) return;

    const navHtml = `
        <div class="bottom-nav">
            <a href="home.html" class="nav-item" data-nav="home">
                <img src="../Img/tab栏/首页-未点.png" data-default="../Img/tab栏/首页-未点.png" data-active-src="../Img/tab栏/首页.png" alt="首页" class="nav-icon">
                <span>首页</span>
            </a>
            <a href="discover.html" class="nav-item" data-nav="discover">
                <img src="../Img/tab栏/发现-未点.png" data-default="../Img/tab栏/发现-未点.png" data-active-src="../Img/tab栏/发现.png" alt="发现" class="nav-icon">
                <span>发现</span>
            </a>
            <a href="information.html" class="nav-item" data-nav="information">
                <img src="../Img/tab栏/咨询-未点.png" data-default="../Img/tab栏/咨询-未点.png" data-active-src="../Img/tab栏/咨询.png" alt="咨询" class="nav-icon">
                <span>咨询</span>
            </a>
            <a href="profile.html" class="nav-item" data-nav="profile">
                <img src="../Img/tab栏/我的-未点.png" data-default="../Img/tab栏/我的-未点.png" data-active-src="../Img/tab栏/我的.png" alt="我的" class="nav-icon">
                <span>我的</span>
            </a>
        </div>
    `;

    containers.forEach(container => {
        const active = container.getAttribute('data-active') || '';
        container.innerHTML = navHtml;

        const navItems = container.querySelectorAll('[data-nav]');
        navItems.forEach(item => {
            const img = item.querySelector('.nav-icon');
            const defaultSrc = img?.dataset.default;
            const activeSrc = img?.dataset.activeSrc;

            if (item.dataset.nav === active) {
                item.classList.add('active');
                if (img && activeSrc) {
                    img.src = activeSrc;
                }
            } else {
                if (img && defaultSrc) {
                    img.src = defaultSrc;
                }
            }
        });
    });
});

