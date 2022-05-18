
// Faculty and Department Array

var Faculty_arr = new Array("Faculty of Science and Technology", "Faculty of Basic Medical Science", "Faculty of Art and Humanities");

var Department = new Array();

Department[0] = "";

Department[1] = "CHEMISTRY/BIOCHEMISTRY|MATHEMATICS/INFORMATICS/COMPUTER SCIENCE/STATISTICS|PHYSIC/GEOLOGY/GEOPHYSIC|BIOLOGICAL SCIENCES";

Department[2] = "ANATOMY/PHYSIOLOGY/MEDICAL BIOCHEMISTRY";

Department[3] = "ACCOUNTANCY/BUSINESS MANAGEMENT/ENTREPRENEURIAL STUDIES|ECONOMICS AND DEVELOPMENT STUDIES|HISTORY AND STRATEGIC STUDIES|POLITICAL SCIENCE/PSYCHOLOGY& SOCIOLOGY|LANGUAGES/LINGUISTICS AND LITERARY STUDIES ";

function print_faculty(faculty_id){

	var option_str = document.getElementById(faculty_id);
	option_str.length=0;
	option_str.options[0] = new Option('Select Faculty','');
	option_str.selectedIndex = 0;
	for (var i=0; i<Faculty_arr.length; i++) {
		option_str.options[option_str.length] = new Option(Faculty_arr[i].toUpperCase(),Faculty_arr[i].toUpperCase());
	}
}


function print_department(department_id, department_index){
	
	var option_str = document.getElementById(department_id);
	option_str.length=0;	// Fixed by Julian Woods
	option_str.options[0] = new Option('Select Department','');
	option_str.selectedIndex = 0;
	var Dept_arr = Department[department_index].split("|");
	for (var i=0; i<Dept_arr.length; i++) {
		option_str.options[option_str.length] = new Option(Dept_arr[i],Dept_arr[i]);
	}
}


//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


var State = new Array("ABIA", "ADAMAWA", "AKWA IBOM", "ANAMBRA", "BAUCHI", "BAYELSA", "BENUE","BORNO", "CROSS RIVER", "DELTA", "EBONYI", "EDO", "EKITI", "ENUGU", "GOMBE", "IMO", "JIGAWA", "KADUNA", "KANO", "KATSINA", "KEBBI", "KOGI", "KWARA", "LAGOS", "NASSARAWA", "NIGER", "OGUN", "ONDO", "OSUN", "OYO", "PLATEAU", "RIVERS", "SOKOTO", "TARABA", "YOBE", "ZAMFARA", "FCT ABUJA");

var LGA = new Array();

LGA[0]="";

LGA[1] = "Aba North|Aba South|Arochukwu|Bende|Ikawuno|Ikwuano|Isiala-Ngwa North|Isiala-Ngwa South|Isikwuato|Isuikwuato|Nneochi|Obi Ngwa|Obi Ngwa|Obioma Ngwa|Obioma-Ngwa|Ohafia|Ohaozara|Osisioma|Ugwunagbo|Ukwa West|Ukwu East|Umuahia|Umuahia North|Umuahia South|Umuahia-North|Umunneochi";

LGA[2] = "Demsa|Fufore|Ganye|Girei|Gombi|Guyuk|Hong|Jada|Jimeta|Lamurde|Madagali|Maiha|Mayo-Belwa|Michika|Mubi-North|Mubi-South|Numan|Shelleng|Song|Toungo|Yola ";
	
LGA[3] = "Abak|Eastern-Obolo|Eket|Ekpe-Atani|Esit Ekit|Essien-Udim|Etim-Ekpo|Etinam|Ibeno|Ibesikpo-Asutan|Ibiono-Ibom|Ika|Ikono|Ikot-Abasi|Ikot-Ekpene|Ini|Itu|Mbo|Mkpat-Enin|Nsit-Ibom|Nsit-Ubium|Obot-Akara|Okobo|Onna|Oron|Oruk Anam|Udung-Uko|Ukanefun|Uquo Ibeno|Uru Offong Oruko|Uruan|Uruk-Anam|Uyo";
	
LGA[4] = "Aguata|Akwa North|Anambra|Anambra-West|Anaocha|Awka-North|Awka-South|Ayamelum|Dunukofia|Ekwusigo|Idemili|Idemili-North|Idemili-South|Ihiala|Imo|Nibo|Njikoka|Nnewi|Nnewi-North|Nnewi-South|Ogbaru|Olumba|Onitsha-North|Onitsha-South|Orumba-North|Orumba-South|Oti|Otu-Ocha|Ubuluizor Ihiala|Uyi ";

LGA[5] = "Alkaleri|Bauchi|Bogoro|Damban|Darazo|Dass|Gamawa|Ganjuwa|Giade|Itas/Gadau|Jama'Are|Katagum|Kirfi|Misau|Ningi|Shira|Tafawa-Balewa|Toro|Warji|Zaki ";

LGA[6] = "Adagbabiri|Brass|Ekeremor|Kembe|Kolokuma|Kolokuma/Opkuma|Nembe|Ogbia|Sagbama|Southern-Ijaw|Toru-Abubo|Yenegoa ";

LGA[7] = "Ado|Agatu|Apa|Buruku|Gboko|Guma|Gwer-East|Gwer-West|Katsina-Ala|Konshisha|Kwande|Logo|Makurdi|Ogbadibo|Ohimini|Oju|Okpokwu|Otukpo|Oturkpa|Tarka|Ukum|Ushongo|Vandeikya";

LGA[8] = "Abadan|Askira-Uba|Bama|Bayo|Biu|Chibok|Damboa|Dikwa|Gubio|Guzamala|Gwoza|Hawul|Jere|Kaga|Kala/Balge|Konduga|Kukawa|Kwaya-Kusar|Mafa|Magumeri|Maiduguri|Marte|Mobbar|Mongunu|Ngala|Nganzai|Shan";

LGA[9] = "Abi|Abuochichie|Akamkpa|Akpabuyo|Bakassi|Bekwara|Biasi|Boki|Calabar-Municipal|Calabar-South|Etunk|Ikom|Obanliku|Obubra|Obudu|Odukpani|Ogoja|Ugep-North|Yakurr|Yala";

LGA[10] = "Aniocha North|Aniocha-North|Aniocha-South|Bomadi|Burutu|Effurun|Ethiope-East|Ethiope-West|Idu|Ika-Ne|Ika-North-East|Ika-South|Ikpemili|Isoko-North|Isoko-South|Ndokwa-East|Ndokwa-North|Ndokwa-South|Ndokwa-West|Okpe|Okwuani|Oleh|Oshielli-North|Oshimili|Oshimili-North|Oshimili-South|Osimili|Osimili-North|Osimili-South|Patani|Sapele|Udokwa|Udu|Ughelli-North|Ughelli-South|Ukwuani|Uraun|Urwie|Uvie|Uvwei|Uvwie|Uwvie|Warri-Central|Warri-North|Warri-South"; 

LGA[11] = "Abakaliki|Afikpo-North|Afikpo-South|Bomadim|Ebonyi|Ezza-North|Ezza-South|Ikwo|Ishielu|Ivo|Izzi|Obaukwu|Ohakwu|Onicha|Ukaba";

LGA[12] = "Afokpella|Afuze|Agbazilo|Akoko Edo|Akoko-Edo|Egor|Esan-Central|Esan-North-East|Esan-North-East|Esan-South-East|Esan-West|Etsako-Central|Etsako-East|Etsako-West|Igueben|Iguobano North East|Ikpoba-Okha|Ohunmwode|Ologbo|Opoji Irrua|Opoji Irrua|Oredo|Orhionmwon|Ovia-North-East|Ovia-South-West|Owan East|Owan-East|Owan-West|Uhunmwonde ";

LGA[13] = "Ado-Ekiti|Aiyekire|Efon|Ekiti-East|Ekiti-South-West|Ekiti-West|Emure/Ise/Orun|Gbonyin|Ido-Osi|Ijero|Ikare|Ikere|Ikole|Ilejemeje|Irepodun/Ifelodun|Ise-Orun|Moba|Oye ";

LGA[14] = "Aninri|Awgu|Enugu-East|Enugu-North|Enugu-South|Ezeagu|Igbo-Etiti|Igbo-Eze-North|Igbo-Eze-South|Isi-Uzo|Nkanu-East|Nkanu-West|Nsukka|Nukanu East|Oji-River|Udenu|Udi|Uzo-Uwani";

LGA[15] = "Akko|Balanga|Billiri|Dukku|Funakaye|Gombe|Kaltungo|Kwami|Nafada/Bajoga|Shomgom|Yamaltu/Deba";

LGA[16] = "Aboh-Mbaise|Ahiazu-Mbaise|Dral-Esat|Ehime-Mbano|Ezeobodo|Ezinihitte|Ideato|Ideato-North|Ideato-South|Ihitte/Uboma|Ikeduru|Isiala-Mbano|Isu|Mbaitoli|Mbano|Ngor-Okpala|Njaba|Nkwerre|Nwangele|Obowo|Oguta|Ohaji-Egbema|Okigwe|Onuimo|Orlu|Oro-West|Orsu|Oru-East|Oru-West|Owerri-Municipal|Owerri-North|Owerri-West|Ugiri-Ike Ikeduru|Ugiri-Ikedikeduru|Unbano|Zinihitte";

LGA[17] = "Auyo|Babura|Biriniwa|Birnin-Kudu|Bosuwa|Buji|Dutse|Gagarawa|Garki|Gumel|Guri|Gwaram|Gwiwa|Hadejia|Jahun|Kafin-Hausa|Kaugama|Kazaure|Kirkasamma|Maigatari|Malam-Maduri|Miga|Ringim|Roni|Sule-Tankarkar|Taura|Yankwashi";

LGA[18] = "Birnin-Gwari|Chikun|Giwa|Gwagwada|Igabi|Ikara|Jaba|Jema'A|Kachia|Kaduna-North|Kagarko|Kajuru|Kaura|Kauru|Koka/Kawo|Kubah|Kudan|Lere|Makarfi|Sabon-Gari|Sanga|Soba|Tudun-Wada/Makera|Zango-Kataf|Zaria";

LGA[19] = "Ajingi|Albasu|Bagwai|Bebeji|Bichi|Bunkure|Dala|Dambatta|Dawakin-Kudu|Dawakin-Tofa|Doguwa|Fagge|Gabasawa|Garko|Garun-Mallam|Gaya|Gezawa|Gwale|Gwarzo|Kano-Municipal|Karaye|Kibiya|Kiru|Kumbotso|Kunchi|Kura|Madobi|Makoda|Minjibir|Nasarawa|Rano|Rimin-Gado|Rogo|Shanono|Sumaila|Takai|Tarauni|Tofa|Tsanyawa|Tudun-Wada|Ungogo|Warawa|Wudil";

LGA[20] = "Bakori|Batagarawa|Batsari|Baure|Bindawa|Charanchi|Dan-Musa|Dandume|Danji|Daura|Dutsi|Dutsinma|Faskari|Funtua|Ingawa|Jibia|Kafur|Kaita|Kankara|Kankia|Katsina|Kurfi|Kusada|Mai-Adua|Malumfashi|Mani|Mashi|Matazu|Musawa|Rimi|Sabuwa|Safana|Sandamu|Zango ";

LGA[21] = "Aleiro|Arewa-Dandi|Argungu|Augie|Bagudo|Birnin-Kebbi|Bumza|Dandi|Danko|Fakai|Gwandu|Jega|Kalgo|Koko-Besse|Maiyama|Ngaski|Sakaba|Shanga|Suru|Wasagu|Yauri|Zuru";

LGA[22] = "Adavi|Ajaokuta|Ankpa|Dekina|Ibaji|Idah|Igalamela|Ijumu|Ikoyi-Ijumu|Kabba/Bunu|Kogi|Lokoja|Mopa-Muro-Mopi|Obaji|Ofu|Ogori/Magongo|Okehi|Okene|Olamaboro|Omala|Oru|Oyi|Yagba-East|Yagba-West ";

LGA[23] = "Asa|Baruten|Edu|Ekiti|Ilorin-East|Ilorin-South|Ilorin-West|Isin|Kaiama|Moro|Offa|Oke-Ero|Oyun|Pategi ";

LGA[24] = "Agege|Ajeromi-Ifelodun|Alimosho|Amuwo-Odofin|Apapa|Badagry|Epe|Eti-Osa|Ibeju-Lekki|Ifako-Ijaiye|Ikeja|Ikorodu|Island|Kosofe|Lagos-Island|Lagos-Mainland|Mushin|Ojo|Oshodi-Isolo|Shomolu|Somolu|Suru-Lere|Yewa-South";

LGA[25] = "Akwanga|Awe|Doma|Karu|Keana|Keffi|Kokona|Lafia|Nassawara|Nassawara Eggon|Obi|Wambu";

LGA[26] = "Agaie|Agwara|Bida|Borgu|Bosso|Chanchaga|Edati|Gbako|Gurara|Katcha|Kontagora|Lapai|Lavun|Magama";

LGA[27] = "Abeokuta-North|Abeokuta-South|Abor Mbaise|Ado-Odo/Ota|Adoodo-Ota|Ewekoro|Idarapo|Iffo-Otta|Ifo|Igbado-North|Igbado-South|Ijebu-East|Ijebu-North|Ijebu-North-East|Ijebu-Ode|Ikenne|Ilishan Remo|Ilugun-Alaro|Imeko-Afon|Ipokia|Lgayekpe|Obafemi-Owode|Odedah|Odogbolu|Ogun-Waterside|Remo-North|Sagamu|Shagamu|Yewa";

LGA[28] = "Akoko|Akoko-North|Akoko-North-West|Akoko-South|Akoko-South-East|Akure|Akure-North|Akure-South|Ese-Odo|Idanre|Ifedore|Igbisin|Ikale|Ilaje|Ilaje-West|Ile-Oluji-Okeigbo|Irele|Odigbo|Oka Ak0Ko|Okiti Pupa Ijuodo|Okiti-Pupa|Ondo|Ondo West|Ondo-East|Ose|Owo ";

LGA[29] = "Atakumosa|Atakumosa East|Ayeda-Ade|Ayedire|Boluwaduro|Boripe|Ede|Ede North|Egbedore|Ejigbo|Ife|Ife North|Ife South|Ife-Central|Ife-East|Ifelodun|Ila|Ilesa-East|Ilesa-West|Ilesha|Ilesha West|Irepodun|Irewole|Isokan|Iwo|Obokun|Odo-Otin|Ola Oluwa|Olorunda|Ori-Ade|Orolu|Osogbo";

LGA[30] = "Afijio|Akinyele|Atiba|Atigbo|Egbeda|Ibadan-Central|Ibadan-North-East|Ibadan-North-West|Ibadan-South-East|Ibadan-South-West|Ibarapa-Central|Ibarapa-East|Ibarapa-North|Ido|Ifedayo|Ifeloju|Irepo|Iseyin|Itseiwaju|Iwajowa|Kajola|Lagelu|Odo-Oluwa|Ogbomosho-North|Ogbomosho-South|Olorunsogo|Oluyole|Ona-Ara|Orelope|Ori-Ire|Oyo-West|Saki-East|Saki-West|Surulere";

LGA[31] = "Barkin-Ladi|Bassa|Bokkos|Jos-East|Jos-North|Jos-South|Kanam|Kanke|Langtang-North|Langtang-South|Mangu|Mikang|Pankshin|Quan'Anpan|Riyom|Shendam|Wase ";

LGA[32] = "Aboa/Odual|Ahoada-East|Ahoada-West|Akukutoru|Andoni|Asari-Toru|Bonny|Buguma|Degema|Eleme|Elfane|Emuoha|Etche|Gokana|Ikwerre|Khana|Obia/Akpor|Ogba-Egbema-Ndoni|Ogba/Egbema/Ndoni|Ogu/Bolo|Okirika|Omuma|Opobo/Nkoro|Oyigbo|Port-Harcourt|Tai";

LGA[33] = "Binji|Bodinga|Dange-Shuni|Gada|Goronyo|Gudu|Gwadabawa|Illela|Kebbe|Kware|Raba|Sabo-Birni|Shagari|Silame|Sokoto-North|Sokoto-South|Tambuwal|Tangaza|Tureta|Wamakko|Wurno|Yabo";

LGA[34] = "Ardo-Kola|Bali|Donga|Gashaka|Gassol|Ibi|Jalingo|Karim-Lamido|Kurmi|Lau|Oilingo|Sardauna|Takum|Ussa|Wukari|Yorro|Zing";

LGA[35] = "Bade|Borsali|Damaturu|Fika|Fune|Geidam|Gogaram|Gujba|Gulani|Jakusko|Karasuwa|Machina|Nangere|Nguru|Potiskum|Tarmua|Yunusari|Yusufari";

LGA[36] = "Anka|Bakura|Bukkuyum|Bungudu|Gumi|Gusau|Isa|Kaura-Namoda|Kiyawa|Maradun|Maru|Shinkafi|Talata-Mafara|Tsafe|Zurmi";

LGA[37] = "Abuja";



function print_state(state_id){

	var option_str = document.getElementById(state_id);
	option_str.length=0;
	option_str.options[0] = new Option('Select State','');
	option_str.selectedIndex = 0;
	for (var i=0; i<State.length; i++) {
		option_str.options[option_str.length] = new Option(State[i].toUpperCase(),State[i].toUpperCase());

	}
}

function print_LGA(LGA_id, LGA_index){

	var option_str = document.getElementById(LGA_id);
	option_str.length=0;	// Fixed by Julian Woods
	option_str.options[0] = new Option('Select LGA','');
	option_str.selectedIndex = 0;
	var lga_arr = LGA[LGA_index].split("|");
	for (var i=0; i<lga_arr.length; i++) {
		option_str.options[option_str.length] = new Option(lga_arr[i],lga_arr[i]);
	}
}

