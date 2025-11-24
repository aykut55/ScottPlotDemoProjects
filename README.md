# ScottPlotDemoProjects
ScottPlot Demo Projects (Multiple Independent Pojects In  The Same Repository)

ÇÖZÜM 1 — Sparse Checkout (Tek klasör klonlamak)

(En iyi ve en çok kullanılan yöntem)

Git’in Sparse Checkout özelliği, bir repo içinden sadece istediğin klasörleri indirmene izin verir.

Bu yöntem hem GitHub hem GitLab hem de lokal için çalışır.

Adım adım — sadece ScottPlotDemo1 klasörünü klonlamak

Repo’yu içeriği indirmeden clone et:
	git clone --no-checkout https://github.com/<kullanıcı>/ScottPlotDemoProjects.git
	cd ScottPlotDemoProjects
	git sparse-checkout init --cone
	git sparse-checkout set ScottPlotDemo1

Sonradan istersen yeni klasör ekleyebilirsin:
	git sparse-checkout add ScottPlotDemo2

Kısaca "Tek klasörü klonlama" komutu
	git clone --no-checkout https://github.com/<user>/ScottPlotDemoProjects.git
	cd ScottPlotDemoProjects
	git sparse-checkout init --cone
	git sparse-checkout set ScottPlotDemo1



