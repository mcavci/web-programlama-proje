document.addEventListener("DOMContentLoaded", function () {

    
    //    AYLIK KAZANÇ GRAFİĞİ (Sadece Admin Sayfasında Çalışır)
  
    var ctxLine = document.getElementById('kazancChart');
    if (ctxLine && window.adminVerileri) {
        new Chart(ctxLine.getContext('2d'), {
            type: 'line',
            data: {
                labels: window.adminVerileri.aylar,
                datasets: [{
                    label: 'Aylık Kazanç (TL)',
                    data: window.adminVerileri.kazancData,
                    backgroundColor: 'rgba(78, 115, 223, 0.05)',
                    borderColor: 'rgba(78, 115, 223, 1)',
                    pointRadius: 4,
                    pointBackgroundColor: 'rgba(78, 115, 223, 1)',
                    pointBorderColor: '#fff',
                    pointHoverRadius: 5,
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, grid: { borderDash: [2] } },
                    x: { grid: { display: false } }
                }
            }
        });
    }

 
    //    HOCA TERCİH GRAFİĞİ (Pasta)
   
    var ctxPie = document.getElementById('hocaChart');
    if (ctxPie && window.adminVerileri) {
        new Chart(ctxPie.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: window.adminVerileri.hocaIsim,
                datasets: [{
                    data: window.adminVerileri.hocaSayi,
                    backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b'],
                    hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617'],
                    hoverBorderColor: "rgba(234, 236, 244, 1)",
                }],
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom' } },
                cutout: '70%',
            },
        });
    }

  
    //   FADE ANİMASYONU VE KART EFEKTLERİ
  
    const animasyonluOgeler = document.querySelectorAll('.card, .p-5, h2, .alert, .row');
    if (animasyonluOgeler.length > 0) {
        animasyonluOgeler.forEach(el => el.classList.add('fide-baslangic'));

        const gozlemci = new IntersectionObserver((girisler) => {
            girisler.forEach(giris => {
                if (giris.isIntersecting) {
                    giris.target.classList.add('fide-ac');
                    gozlemci.unobserve(giris.target);
                }
            });
        }, { threshold: 0.1 });

        animasyonluOgeler.forEach(el => gozlemci.observe(el));
    }


    //. DARK MODE AYARLARI

    const toggleBtn = document.getElementById('darkModeToggle');
    if (toggleBtn) {
        const body = document.body;
        const icon = toggleBtn.querySelector('i');

        if (localStorage.getItem('darkMode') === 'enabled') {
            body.classList.add('dark-mode');
            if (icon) { icon.classList.remove('fa-moon'); icon.classList.add('fa-sun'); }
        }

        toggleBtn.addEventListener('click', () => {
            if (body.classList.contains('dark-mode')) {
                body.classList.remove('dark-mode');
                localStorage.setItem('darkMode', 'disabled');
                if (icon) { icon.classList.remove('fa-sun'); icon.classList.add('fa-moon'); }
            } else {
                body.classList.add('dark-mode');
                localStorage.setItem('darkMode', 'enabled');
                if (icon) { icon.classList.remove('fa-moon'); icon.classList.add('fa-sun'); }
            }
        });
    }

   
   
   
    const calendarContainer = document.getElementById('calendarDays');

  
    if (calendarContainer) {
        let currentDate = new Date();
        const randevuUrl = "/Randevu/Al";

       
        const prevBtn = document.getElementById('prevMonthBtn');
        const nextBtn = document.getElementById('nextMonthBtn');

       
        renderCalendar();

     
        if (prevBtn) prevBtn.addEventListener('click', () => changeMonth(-1));
        if (nextBtn) nextBtn.addEventListener('click', () => changeMonth(1));

        function changeMonth(direction) {
            currentDate.setMonth(currentDate.getMonth() + direction);
            renderCalendar();
        }

        function renderCalendar() {
            const year = currentDate.getFullYear();
            const month = currentDate.getMonth(); 

            
            const monthNames = ["OCAK", "ŞUBAT", "MART", "NİSAN", "MAYIS", "HAZİRAN", "TEMMUZ", "AĞUSTOS", "EYLÜL", "EKİM", "KASIM", "ARALIK"];
            const headerEl = document.getElementById('currentMonthYear');
            if (headerEl) headerEl.innerText = `${monthNames[month]} ${year}`;

            
            calendarContainer.innerHTML = '<div class="text-center w-100 py-5" style="grid-column: span 7;"><div class="spinner-border text-primary" role="status"></div></div>';

          
            fetch(`/Home/GetDolulukDurumu?year=${year}&month=${month + 1}`)
                .then(response => {
                    if (!response.ok) throw new Error('Veri çekilemedi');
                    return response.json();
                })
                .then(data => {
                    calendarContainer.innerHTML = ""; 

                   
                    let firstDayIndex = new Date(year, month, 1).getDay();
                    firstDayIndex = (firstDayIndex === 0) ? 6 : firstDayIndex - 1; 
                    const daysInMonth = new Date(year, month + 1, 0).getDate();

                   
                    const today = new Date();
                    const isCurrentMonth = today.getFullYear() === year && today.getMonth() === month;

                  
                    for (let i = 0; i < firstDayIndex; i++) {
                        let emptyDiv = document.createElement('div');
                        emptyDiv.className = 'day-empty';
                        calendarContainer.appendChild(emptyDiv);
                    }

               
                    for (let i = 1; i <= daysInMonth; i++) {
                        const gunVerisi = data.find(d => d.gun === i);
                        const statusClass = gunVerisi ? gunVerisi.renk : "bg-success";
                        const statusText = gunVerisi ? gunVerisi.durum : "MÜSAİT";

                        let dayDiv = document.createElement('div');
                        dayDiv.className = 'day-box p-2 border rounded text-center position-relative';
                        dayDiv.style.minHeight = '80px';
                        dayDiv.style.backgroundColor = '#fff';
                        dayDiv.style.transition = 'all 0.2s';

                        
                        let isPast = false;
                        if (year < today.getFullYear()) isPast = true;
                        else if (year === today.getFullYear() && month < today.getMonth()) isPast = true;
                        else if (isCurrentMonth && i < today.getDate()) isPast = true;

                        if (isCurrentMonth && i === today.getDate()) {
                            dayDiv.style.border = "2px solid #0d6efd";
                            dayDiv.style.backgroundColor = "#f0f8ff";
                        }

                        if (isPast) {
                         
                            dayDiv.style.opacity = "0.6";
                            dayDiv.style.cursor = "default";
                            dayDiv.innerHTML = `<div class="fw-bold fs-5 text-muted">${i}</div><span class="badge bg-secondary mt-2 w-100" style="font-size:0.7rem;">GEÇTİ</span>`;
                        } else {
                           
                            dayDiv.style.cursor = "pointer";
                            dayDiv.setAttribute("title", "Randevu Al");

                           
                            dayDiv.innerHTML = `<div class="fw-bold fs-5 text-dark">${i}</div><span class="badge ${statusClass} mt-2 w-100" style="font-size:0.7rem;">${statusText}</span>`;

                            dayDiv.addEventListener('click', function () {
                                let m = (month + 1).toString().padStart(2, '0');
                                let d = i.toString().padStart(2, '0');
                                window.location.href = `${randevuUrl}?tarih=${year}-${m}-${d}`;
                            });

                            
                            dayDiv.onmouseover = function () {
                                this.style.transform = "translateY(-3px)";
                                this.style.boxShadow = "0 5px 15px rgba(0,0,0,0.1)";
                                this.style.zIndex = "10";
                            };
                            dayDiv.onmouseout = function () {
                                this.style.transform = "translateY(0)";
                                this.style.boxShadow = "none";
                                this.style.zIndex = "1";
                            };
                        }

                        calendarContainer.appendChild(dayDiv);
                    }
                })
                .catch(err => {
                    console.error(err);
                    calendarContainer.innerHTML = '<p class="text-danger text-center w-100" style="grid-column: span 7;">Takvim verisi yüklenemedi.</p>';
                });
        }
    }
});

function openPaymentModal(paketAdi, fiyat) {
    const modalAd = document.getElementById('modalPaketAdi');
    const modalFiyat = document.getElementById('modalFiyat');
    const modalEl = document.getElementById('paymentModal');

    if (modalAd && modalFiyat && modalEl) {
        modalAd.innerText = paketAdi;
        modalFiyat.innerText = fiyat;
        var myModal = new bootstrap.Modal(modalEl);
        myModal.show();
    }
}