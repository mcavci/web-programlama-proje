
$(document).ready(function() {

    // Hoca veya Tarih deðiþtiðinde bu fonksiyon çalýþýr
    $("#drpEgitmen, #dtpTarih").change(function() {

        var secilenHoca = $("#drpEgitmen").val();
        var secilenTarih = $("#dtpTarih").val();
        var saatDropdown = $("#drpSaat");

        // Eðer hem hoca hem tarih seçildiyse iþlemi baþlat
        if (secilenHoca && secilenTarih) {

            // 1. Kullanýcý beklerken bilgi ver
            saatDropdown.empty(); 
            saatDropdown.append('<option value="">Saatler Hesaplanýyor...</option>');
            saatDropdown.prop('disabled', true); 


            // 2. Sunucuya (Controller'a) sor
            $.ajax({
                url: '/Randevu/GetMusaitSaatler',
                type: 'GET',
                data: { egitmenId: secilenHoca, tarih: secilenTarih },
                success: function(gelenSaatler) {

                    saatDropdown.prop('disabled', false); // Kilidi aç
                    saatDropdown.empty();
                    saatDropdown.append('<option value="">-- Musait Saati Seciniz --</option>');

                    
                    if (gelenSaatler.length > 0) {
                        $.each(gelenSaatler, function(index, saat) {
                            saatDropdown.append($('<option></option>').val(saat).html(saat));
                        });
                    } else {
                        saatDropdown.empty();
                        saatDropdown.append('<option value="">Bu tarihte tüm saatler DOLU!</option>');
                    }
                },
                error: function() {
                    alert("Saatler getirilirken bir hata oluþtu! Lütfen sayfayý yenileyin.");
                    saatDropdown.empty();
                    saatDropdown.append('<option value="">Baðlantý Hatasý</option>');
                    saatDropdown.prop('disabled', false);
                }
            });
        }
        else {
         
            saatDropdown.empty();
            saatDropdown.append('<option value="">-- Once Egitmen ve Tarih Seciniz --</option>');
        }
    });
});
