using System.Collections.Generic;
using Universal.Phonetic.Metaphone;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.IO;

public class SlurDetectorEvan : MonoBehaviour
{

    // public   string[] whiteList = { "ounger", "fuckit", "fuck it", "donkey", "spongebob", "ing", "ang" };


    private string[] whiteList = {"rick", "morty", "code bullet", "codebullet", "yoda" , "jar jar binks", "spongebob", "patrick", "shrek", "donkey", "squidward",
            "jerry", "jar jar", "homer", "bart", "marge", "no ", "anger", "give", "enjoy", "just",
            "ounger", "fuckit", "donkey", "spongebob","one","can ","ong", "eng", "ing", "ang", "fuck", "forgot", "fog", "rattlesnakecurled", "snowground", "forget", "folk", "vague", "faked", "forgit", "fidget", "fugit", "fucked", "fig", "forged", "forked", "fidgeted", "vogue", "naca", "nick", "faugh", "nigh", "graveclothes", "advocating", "fidgets", "overjoyed", "drawingroom", "language", "nickel", "nowgrew", "kearneyguarantor", "explainedjurists", "figures", "angry", "inaccurate", "knowcreeping", "fortification", "provoked", "increasing", "naked", "snowyground", "affections", "forgotten", "fugitive", "diningroom", "knocker", "strangenesscrowded", "onecurious", "fac", "navigation", "advocated", "congratulate", "moneykeep", "remaingreatest", "explanationcourtesied", "canegermain", "vigorously", "vegetables", "ironical", "affectionatelygruff", "annakarenina", "beengermany", "navigate", "fidgety", "nowgeorge", "navigationborrowing", "navigatorsthere", "navigators", "vegetation", "vacate", "viget", "neque", "nega", "imperfect", "evacuated", "increased", "unique", "neck", "suffocated", "communication", "congratulates", "nowgrown", "moneycourse", "perfection", "incongruity", "inequality", "ironygreater", "concerningcorruption", "perfect", "incredible", "vegetable", "knocking", "suffocating", "moneygreatly", "voyagewhat", "ingroia", "negative", "minecross", "figure", "communicate", "fugue", "evacuate", "ingratiating", "ceremonygreeting", "donnegare", "victim", "incredulously", "communicative", "affectionately", "moneycarte", "difficult", "accompanyingcreaking", "nowcorpse", "unvexed", "virginiagreat", "outdonecourtesied", "explaininggeorge", "vacation", "vacated", "doorknocker", "wanecrossed", "naekernan", "manacled", "hennycornmeal", "hungrier", "nogregor", "nowgirls", "phocid", "invoking", "ingredient", "sinecurists", "jejunegrammarians", "divinequarrels", "knewjardans", "scientifically", "forgetteth", "ravaged", "copenhagen", "healfdenegrandfather", "alonecoward", "vic", "veg", "ceremoniouscard", "fugitives", "scenecursed", "frankenstein", "divinecuriosity", "vexed", "cynical", "advocates", "congratulatory", "communications", "miscellaneouscriminals", "accompanyingcarriage", "ceremonyjourney", "vegetating", "knickerbocker", "chimneycorner", "affection", "continuegrow", "fugge", "vagitibus", "montaignegreat", "gournaycarried", "diverged", "janecry", "stonecourage", "explanationjerked", "concrete", "newcomer", "outlinecreated", "booklearningcuriosity", "difficulty", "honeycoloured", "knowcried", "singiordano", "ludvig", "evoked", "engraver", "nowcrown", "donecharacterize", "fundevogel", "snake", "nowgrinder", "alonejorindel", "effect", "niccol", "brittanycrown", "romagnacardinal", "colonnagiorgio", "necho", "subvocalization", "encreased", "negations", "latinecredo", "companycare", "caprinajurisdiction", "vigorous", "fidgetiness", "nowcriminals", "fearlessnesscourageous", "tchermashnyajourney", "biographical", "revenuegrounded", "bandanacarefully", "nowjeer", "confectioners", "engravings", "spinecreature", "nocardsof", "viscount", "monico", "anthonygreeting", "savannahcareful", "fact", "knowgrowled", "knowcourage", "traffic", "appertaininggreeting", "invoked", "forgits", "knowgrit", "armonygarding", "minajournal", "encreaseth", "satisfaction", "ingredients", "vinegar", "ingross", "communicated", "manygrand", "fairfax", "vegetarians", "moncrieff", "navigating", "forgetfulness", "anyquarter", "nogracias", "tonecuriosity", "affected", "manycurious", "childrengeorge", "necki", "factitious", "sneaking", "nighcrushed", "trivialnesscarrying", "selfexaminationdentistrythe", "bankrupted", "picnichuck", "lonelinesscry", "sinaigirded", "chronicles", "newgroove", "accompaniedcurate", "mahoganygeorge", "lifecouldnt", "scrutinygreatest", "neckwhy", "waitingroom", "knowcoming", "monotonousgrimness", "bonyjerks", "engraved", "nicholas", "moneycriedmercy", "douniacourse", "overcoat", "sittingroom", "concernedgrievance", "newjersey", "befogged", "unaccountably", "congress", "catherinecrazy", "bankrupted", "fidgetty", "communicating", "journeyjourney", "vegetarian", "chronicle","fack","fake","faq","fargite","fargood","fauch","feague","feak","fec","fecche","feck","fecket","fehq","fic","ficche","ficoid","figge","fike","fyke","fikh","fioc","fiqh","fique","firecoat","foac","foc","foch","fogg","forcat","forcut","foregate","foregut","forgat","Forgett","Forgette","forkhead","foucquet","fougade","fougue","fouke","fouque","fouquet","fowk","fucate","fuckwit","fucoid","fug","fugate","fuget","fughette","fugued","furcate","gnocchi","hvac","knackaway","knacky","knaggy","knockaway","mngr","nco","naco","naga","naggy","nagoya","nako","nakoo","nca","ncaa","ncga","ncr","neckhigh","neoga","neogaea","ngoko","nica","nicaea","nickey","nicki","nicky","nickie","nicko","nico","nigua","niyoga","nika","nikau","nike","niki","nikki","nikky","nikkie","nikko","niko","noecho","nogai","nogo","nooky","nookie","nucha","nuchae","nugae","phacoid","pherkad","phocoid","phugoid","phuket","vac","vacuate","vacuit","vag","vagueeyed","vegete","veillike","vick","vicoite","viduate","vig","virgate","voc","vocat","vocate","vocoid","vog","voq","vougeot","vug","vugg","vugh","aas","abovenamed","allflowerwater","effigy","farforthly","agee","abaddon","aaron","chremzelchremzlach","caeomacaeomas","geopotentialgeoprobe","vergeht","fuchs","vergeht","hungrigen","junger","zuneigungen","machengar","unconsciously","affectionate","engraving","linguists","winecru","worldlinessguard","vegetate","increases","singer","donegal","anygreener","pennycartons","advocate","incongruous","singular","inauguration","meanigreat","tonecurse","vocal","ifauacuterdammjani","forkeddefp","overjoyedp","engravers","udabbingu","itappenigrope","linecorresponds","functionaryjurymandefp","conviction","advocate","uncle","strunachar","insomniacriticism","uncovered","inoculate","nogrimly","geogrficos","equivocado","viajado","engruesa","lengua","clnica","llanocrecida","vacunocordura","imperfectly","bloodincrusted","encouraged","greenoughcrawford","miscellaneouscuriosities","unaffected","forgetful","increasd","unconfirmd","unequal","nogradual","rapinecarnage","forgetmenots","bungay","iniquity","gonecrowded","scrutinisecarefully","anycolonelgeorge","corngrowing","fingers","raineygreywhiskered","nowcoarse","neckhow","encouragement","manycolored","companycreatures","goodmorningcurtseys","vgg","noga","vik","neka","verket","vg","vaggad","frgat","vg","vik","foglarna","vaggad","verket","ngra","ngar","nickande","armarnakors","fick","fick","vg","vakat","vg","vakade","nog","folket","noga","folket","nyck","vg","vg","vg","veckade","gnaga","folket","folket","knoga","fogade","fogat","neka","frgade","fga","bevakat","tgvirket","svajat","ngra","kungabarn","nakenhet","grefvinnangrefven","rttrognakyrkans","skrifnahjrteligen","vieweg","prefix","gangrene","gangotri","nicolas","religionemchristianam","gallicanismgarretting","gascoignegeorge","increase","bookingoffice","negotiationslord","invokes","overcoats","mingled","inaugurated","guaranigwahrahnee","folks","angrily","inquired","nonegrave","anyqueried","incarnation","iniquity","straighteningcrookedness","intensenesscare","farcould","fogs","farcould","thinking","unawakened","knewcrossed","journeycarts","halifax","flavicat","bifurcata","increasingly","elenchus","botanical","illdefinedcrude","germanycarolinas","concerninggyrosum","crucifixion","congregation","synagogue","nocrowded","agonygarden","bethanyjerusalem","scientific","forgetting","increase","mankind","unacquainted","chimneygrate","unforgotten","francoyanko","funnycrooked","anyonecarry","aeroplanegermans","single","newcartridge","savannahgeorgia","forgetting","triangle","manycoloured","virginiacreeper","nowcorridor","donejeered","affect","single","alonecreatures","affectionatecordial","genoajerusalem","suffocation","forgottenmr","encouraged","examinationsgrinned","nowcorregio","fingers","nakud","naycrime","nowcarefulyou","figeait","prfecture","vgtation","figeait","sincre","chang","nagure","continugrondait","destinecarnage","divisionamiraljaurguiberrycelle","nawgo","fiction","equivocated","increase","finger","mechanical","fraternitycreate","whenqueried","explanationjournalist","nhk","nkyi","nki","nhk","nek","verkot","nk","viikkoa","vakuutettu","virkatoverini","helsingiss","hannikainen","siaanikirjoittajaksi","tyydyttnytjuuri","prefects","lingered","pianocricket","nowcurtis","increase","winked","mechanical","conquer","nothingnessgreenish","hancock","inaugural","greenegreene","companycargoes","vcut","naquis","figurer","vcut","ingrats","annonc","inaugurant","humainecrature","venucouronner","effected","younger","technical","snowcrust","manycarried","carolinageorgia","folk9","youngest","knewgreek","creationcoarse","onejer","fishingrod","unconventional","newgate","wilsonicrankistmons","dooryardneatnesscharacteristic","fergit","figured","fergit","kinkaid","laconically","ingrated","oftencuriously","newjerseymade","facts","concretely","nichols","anygroup","reginacara","antagonismgermany","effects","forgetting","increase","longer","mechanically","knowcardoville","livingroom","dingy","bonycrop","postponedcarefully","sleepingroom","longer","iniquity","imaginecrave","susannahcourse","revengeth","nakedness","voyagedamrosch","congregation","belonging","domenico","ascensionischristi","newcurious","encountered","mechanically","fortunecried","onecurtesey","moneyjournalist","vegghiate","fuggite","nacqui","veggiate","figli","invecchiato","afforcato","beffeggiato","ingrassarla","mangone","maneggi","bisognocrepo","affannocordoglio","crucifixes","engraving","thinking","onegratis","bretagneguards","nowjours","veek","veek","veek","veek","fiction","fakedup","congregation","zangwill","synagogue","pennycried","concernedandcourtyard","pennygeorge","fixity","forgetting","anguishes","knockabout","newgreen","figs","single","unhooking","drawingroomcurious","singular","peronnecrevans","naycare","ascertainedjourney","graphic","lifecould","ungathered","unequal","sunshinegreen","newquarterly","nowjournalists","faculty","overcoats","increasedall","jingles","nokomis","funnycreatures","advocate","increase","longest","unaccountable","congratulated","fingers","nego","figura","advogados","congresso","longa","unica","uniogrupos","nagare","correccionaljuryo","fuggite","fugg","vergate","fugg","neghi","vaghe","figit","nacqui","niego","niego","nieghi","fugg","rifuggendo","lavvocato","vergate","vegetazione","congresso","arringo","negativo","orthwinogratio","aveanocure","comunegiurisperiti","fiction","boardinghouse","mechanically","helenecroaked","nogurgled","fergit","fixed","forgetting","belonging","snicker","nowcraned","continuejourney","faugh359","marivaux","advocate","triangulate","eugniegrandetle","snobbishnesscorinnes","nojurisdiction","vaag","vak","vergat","vergood","voeg","vakbroeders","advocaten","overgaat","voorgedragen","heerengracht","nottinghamshire","newgate","diegenegraans","beteekenisgordels","kleinegeriefelijke","firkked","firkked","firkked","science","lock","audience","winter","proportion","where","charter","elevatortelephone","statenow","playdown","advocate","encrusted","hungered","inaccuracies","nocried","nocarping","santayanageorge","twinkle","unaccountable","tonegroaning","nocars","lifeclough","advocate","congratulations","concord","negotiations","tennysonworkcromwell","junecarey","socinianismgeorgesandism","scientific","fucata","furcatus","goldencrowned","woodsingers","donacobius","knowcreamcoloured","limnorniscurvirostris","difficulties","vacuity","hungry","encounters","mechanical","voluminousnesstillcrack","tendernesscurved","invigorating","increase","prolonging","againaccording","inordinategratification","winecarnell","vagit","fig14","figuras","fgado","envergadura","vegetativa","engrossamento","encontrar","tnico","senocrescer","raciocniocaractersticas","affectionate","advocate","forgottenfrom","ungrateful","sanguine","negotiations","opportunitycare","foxes","hungry","twinkle","lanecarries","increaselesley","kingedward","unacquainted","opportunitygratifying","tolqhwonegordons","humanajure","vaikket","nki","nuku","nukkua","nkyi","nukkui","nkee","varjot","nkyy","nukkuu","nkee","nki","nukkui","nky","vaikeaksi","vaikket","varjot","helsingiss","siinkin","minkerron","siinjrke","increase","bringing","nocturnalgroup","phalnaquercus","perfectly","suffocate","organgrinder","longer","negatively","powerlessnesscreate","journeyjerusalem","fiction","ungreek","singular","wanegrows","donekurri","fourgeaud","fourgeaud","factor","navigated","fugitivepersecution","congregated","stronger","unoccupied","preternaturalcreatures","francenothingquarrels","increasemoses","ingersoll","inauguratedxiii","destinycreator","womanacourts","shonejourneyed","prefixed","loveguided","languid","newgracd","shonecarest","victims","fidgeting","hungry","engage","unaccustomed","telephoningcrossexamination","collodionizedcoarse","neg","niega","nucay","nucay","efecto","navegado","figitur","concretndonos","cinco","crnica","caminograndeza","humanacuriosidad","forgetting","longer","soncried","significance","angrily","thinking","inconsiderate","newcomers","arenagrain","nocoercion","nonejersey","scientific","bifurcating","monocular","renaultgroldenberg","opportunitiescorrespondence","virginia64jurassic","foquet","fiction","mingled","newcrown","lignycouriers","ungrammatical","bungalee","nowcrew","necke","necke","nicke","fictions4","tinker","faynegramarsey","journeyguard","tenegyrde","difficulties","bankrupt","banquet","nowgreatest","thornecurate","commissionerjorissen","factions","advocate","lincoln","inaugural","manygrave","knowcare","companygirard","incumbent","inaugurated","rightfulnesschristian","haynecarolina","virginia8georgia9","nuku","nki","nki","nkyy","nkyi","nukkui","nukkui","nkee","varjot","nky","nykki","nyki","nkyy","nuku","nukkuu","nuku","nukkua","nahka","nuku","nuku","naukui","pivkaudet","pivkaudet","virketty","vjt","varjot","tingassa","heinkuuta","pienikertomuksia","painojrtti","factionsirish","advocateher","inculcates","mechanical","hoodborneoagrovepontianachinese","monaquartermaster","victor","dovecote","increase","single","technical","nocretins","affectionatelygirl","facts","engrossed","mankind","examiningchristmas","commissionerscorpus","hungry","lincoln","northernercrowded","companygarment","neki","neki","nki","nki","fekdt","nki","fekete","neki","fejt","nki","fogod","nki","nkie","vgad","vgt","fogat","vget","vgt","fejet","neki","fejt","vgott","vgit","fejt","fiok","neki","fkot","fk","fejit","neki","vg","vgett","fiok","vak","vgett","vgett","fgghet","vg","fogyott","fokt","knyvk","megfogat","forgottak","vgytanak","fergetegek","hasznunkra","ezenkivul","neki","mentenkrisztus","asszonykorhoz","eltlnijrjk","facts","provocative","longer","mechanical","dennycried","vicq","fictions","facade","sallengre","buckinghams","scene3granger","scenecarefully","progenygermany","neigh","satisfactory","congregation","lippincotts","technical","newcreated","saintlinessgeorge","fox","smokingroom","inkthe","iniquities","highhandednessquarrels","incorporation","technical","commissionerscourse","faithfulnessjournals","finger","unequalled","overcoats","encountered","goodmorninggreeted","nocarries","congresscombat","sinking","moneycommissioned","donecourtmartial","knocke","revoked","fingers","unequal","winecrockery","nocorrected","vacating","distinguished","negotiate","commissionergrand","nowcare","necka","furgot","folks","buryinground","melancholy","mechanically","wanekerryin","fixed","swimmingindeed","unaccountable","moneygrown","donequeried","nach","mehrfach","totengrberszene","klingenden","kopenhagener","eugeniegraudet","niegarnrllchen","congratulations","inconveniences","fixings","longer","satisfactory","belonged","unacquainted","accompaniedcromlech","manehcarchemish","safeguard","bankruptcy","incapable","techniques","unfortunate","desertum","200203","cut","s","jerome","nonelord","celleretholding","tribesdetailed","noka","effective","languages","aeroplanescritics","newcourse","pacific","bankrupt","bankinghouses","knewcrossing","fock","fiction","congregation","birmingham","mechanical","snowycravat","medicinebottlecork","ninetyninejar","foxes","advocate","incongruous","stingaree","snowgreaser","unseencarpenter","advocate","increase","incometax","opportunitiesgreater","sydneycarpentaria","knowjournal","readingroom","uncle","knowcryin","tinygirl","pianosurely","knock","forgoten","idiosyncracy","lingering","chronicled","manychronicled","moneycourting","fortunecrone","consciousnesscarlos","norfolk","devigataj","verkita","vojetoj","congratulations","single","iulandanojkrom","bezonokorektos","unujaro","fiction","longer","indefinitelygreek","encampment","chattanooga","indianacrab","cynthianagarrisoned","fixedly","ungrateful","inconsistencies","renegades","antagonistshegriefdie","accompanycarriage","continuejourney","victor","congratulations","uncas","vernacular","newgrowing","stonecarving","philippinesjourney","fingers","unaccountable","nocharacter","encrusted","single","gonegrows","newcourt","nowgo","forgetand","fidgetted","banquet","nugget","knewgrandfather","knowcourted","gonesurely","ungraceful","incapable","knuckle","noneyoucare","nuque","vocabulary","ladvocat","linguistic","nicolas","personagrata","voluminouscorrespondence","destinejours","overcoats","single","snowcapped","phenomenagrowler","allnocars","viglins","increase","kingattitude","negotiations","spontaneouscourage","victoria","vacatedthe","angrily","engaging","anachronismgrandfather","nowcarried","scrawnygeraniums","effects","engaged","honeygroaned","onegirl","folks","inglenook","unoccupied","accompaniedgrace","gentiangarden","johnniegeorgie","fixing","hungry","thinking","vaug","vaug","necke","fixiert","navigator","swaengroningen","dunkel","nachweisen","skinnecrisp","buffonecharacters","brenjourneyman","forgetting","inconvenience","knickerbockers","knewgrock","insomniacorrespondence","significant","thinking","mechanical","nocries","politenesscourtesy","significant","inquisitive","technical","imaginegravedigging","alonecurious","vaak","vak","vaak","vak","vergat","vergeet","vergat","vak","vergoed","vacantie","vergat","vergeten","frankrijk","opwellingen","nagel","dunnegordijnen","forget7","vaque","safeguard","conquest","newcomers","opportunitygratefully","winegirls","forgetmenot","hanging","unicorn","nocareful","manufacturers","hunger","unaccountable","nonegreat","meridionaljourney","nac","satisfactoria","bifurcada","refugiado","incrust","conquistador","crnicas","indianograciasle","sanocorazn","pacific","incorporated","angles","funnygrain","finecurly","figs","fidgeting","conquer","nowgrocer","engraven","languidly","ceremonialsground","questiongirl","thankye","nuggets","snowcracking","donecarefully","continuejourney","qualification","bankruptcy","singularly","technical","knowncorrespondence","raincrow","swinging","nowcreeps","perfectly","provocative","ungracious","lancashire","ghastlinessgrin","fannycoarse","increases","longhidden","tenacre","dinahcraik","minegirl","fiction","suffocate","younger","nicked","grannycried","manygirls","johnnysure","avec","lavocat","lingratitude","doncques","pouvonsnouscorinthe","naquis","avec","avogadori","fugitif","vaincre","prolongement","ingales","condamnentcriminels","jenecorporelles","victims","income","ironically","fortunegratify","moneycowardly","necka","cairncross","bunker","manacles","tinygirl","accompaniedjourney","manufacture","thinking","opportunitygrateful","linecorner","bennygeorge","effectively","incorporated","onequarter","accompanycrouched","groancuriosity","pacific","navigator","forgetting","engaged","nogrumbled","kneeicourt","vigoureux","provocative","forgottenhappened","longhandled","veronica","downgrowing","opportunitiesgardening","alonejourney","facts","lincolns","newcastleupontyne","moneygranting","accompaniedcolonel","fogwhat","provocative","incurred","nagging","routinegreat","sanacorpore","thoughtfulnessgermany","mehrfach","vorgetragene","concreten","erlangen","vereinigung","figs","increases","incorporate","lornacordonnier","encounter","negotiate","linecreating","invasionwhencarrier","vake","facts","congressional","mingles","inaugurated","brunogreat","examinedcare","folks","hungry","unkindly","newcomers","sunshinecrickets","brentanoscar","funnyjerusalem","significant","uncle","knowcrazy","examiningcarefully","knowsure","factos","invocados","vegetal","incredulaha","encontrar","naquella","tornochryptomerias","soberanaquero","montanhajorrava","effects","provocative","ingratitude","conquer","knowgrave","minequarrel","classification","viiicataloguing","idiosyncrasies","incomplete","mechanical","botanycryptogams","knowcurious","sansonigerman","stronger","manaclescareful","nowgrappling","nowcornered","neg","niego","neg","fjate","fogosos","sofocado","viejodijo","increble","prolongadas","relacionesgrande","llenacarnes","negou","advogado","advogado","forjados","incrivel","encontrar","negativas","dignograndiosas","mariannocarvalho","turnojornalismo","drinking","mechanically","forgets","increase","lincoln","pinnacle","companychrist","examinationcareful","fogh","ungratious","wringing","nichols","ownohgroan","prisongeorge","suffocate","bankruptcy","oceangoing","wanigan","knowgrandmother","stickneycourse","figg","vagaries","fucata","lancaster","vicinacremon","punyquery","nogeorge","ngayoi","ngayoi","nagaua","nauiuica","ngi","nagca","niuiuica","nauica","nagca","vocal","congregayerunt","manga","nauucol","nacruz","finocaratihan","conchology","unacquainted","heliconidescressida","nowcharacterize","ptiliogonysjourn","shingled","unaccustomed","onecritically","newcarpet","fixed","ingrained","singing","nikato","donegreat","majanocorn","bringing","snaking","nocrazy","destinycharacterised","kootaniegeorge","certificate","hungry","single","carolinacurious","nikya121","nikya254","nikya","naiyyika","nikya372","nikya","geographically","safeguiding","ankara","ngrjuna","nirvanagreater","manycharacteristics","newjerusalem","effects","forgetting","congreve","incomparable","newcomers","sunnycriticise","unfemininecoarse","fix","sinecure","manygranted","naycarrying","longing","nogreet","nogarlanded","vagged","vagged","thinking","sinecure","nawgrowled","snowcorner","gaffneysurething","dovecote","congressnapoleon","francoaustrian","pinnacle","newcross","nocarlyle","ninaigerardine","havock","strongest","knock","vacancy","inconsiderate","oneghost","nogreat","ficam","navegados","engrandecido","engenhosos","unico","crnograndes","lunaquereis","mankind","chinagreensted","octagonalcorona","vicomte","advocate","languedoc","unaccomplished","nogreater","explanationcourier","scientific","anchor","mechanical","colonygroups","enginegear","necke","victorie","increase","concubines","fornication","moniegranteth","christanitiecurtailing","victory","advocate","hungry","encouragement","nonecareful","soongerman","vcut","naquis","avec","vcut","viergetransformation","lingratitude","encore","technique","connugrecque","domainecaractre","forgetting","winging","niagaras","vorgeht","vorgeht","feig","vorgeht","einfach","einfachheit","vorgeht","degengriff","angara","vergnuegungen","silbernenkreuz","verhaeltnissecouriers","graphic","hungry","cunninghame","technicalities","no77cruz","chinagirls","increase","younger","thinegrief","denycare","encounter","punicus","perpennacrassus","colonycarteia","limetanusjurycommission","difficulties","encrusted","quadrangle","nowcraftsmen","magnacarta","satisfactory","longer","knewgarden","fyked","forgets","cabingriffith","melancholy","snakes","journeycrossing","manycareful","hoodwinked","dominican","donegravely","conquests","manycolored","agonycross","verjagt","vorgeht","20ngr","15ngr","eingezogen","arzt","haarlosen","reiche","peitschte","fu","einem","weihnachten","innigereneue","fangzhnenmachen","manufactured","angrily","ankle","nichols","journeygrow","anywaycaretaker","faculty","longer","knickerbockers","nogreat","vaikk","vaikk","vehkeet","nukke","nky","nkee","nk","nk","nkee","nky","nuku","nky","nukkuu","vaikk","vaikk","nki","vkivalta","vehkeet","henkilt","nkyviss","sinikorkealle","vanquished","folks","bunkers","snuggled","nocried","manygirl","navegaes","suffocada","angra","grangear","naquellas","helenagravuraa","resignoucargo","advocate","fidgetting","birmingham","nocharacter","fergit","manufacturers","fergit","hoddengray","lincolnyou","iniquity","bonegrowled","ceremonialguardian","fickle","engaged","knowgracious","nonocourse","nacqui","fugg","niego","fuggite","fugg","nacqui","qualificato","fuggitivi","varcato","vajuti","ringraziando","ancora","domenica","veranogrosso","segnocuriosit","pianigermogliare","folks","lodgingroom","lodginghouse","nowchristie","advocate","incognito","niccolas","companycarbineers","forgott","invokes","dovecotes","vegetationthe","hungry","engagement","unoccupied","attorneygrays","newcarpets","ungrudging","dwellinghouse","neckarside","uncannygravity","journeycarlsruhe","fingers","nowcrumbs","usefulnessgirl","naukhu","nawaoga","nawoga","fixed","ingraham","ingalsbe","nichold","denominationschristian","nerneycourtesies","brandywinegermantown","theosophical","advocate","vegetative","uncle","seneca","gluttonygormandizing","facto","avogado","encontram","chronicas","baminachristos","abbunakerilos","ingratitude","languages","ironically","monotonousgrave","winecareful","nki","nki","nky","nkee","nahka","nkyy","nky","nukkui","nukkui","nukkuu","nukkuu","nukkuu","nukkua","nukkui","nukkuu","nuku","nukkuu","nukkuu","nukkua","varjot","nkee","nukkua","nky","nkyi","nkki","nykii","varkaat","viehket","voihkit","vaikka","vaikeata","varkauteen","aaveajatuksen","varjot","helsinki","nekksti","veljenikristuksessa","romaanikirj","ernjrkiins","singers","synagogue","agonycross","consciousnessjerusalem","vergeht","ficht","vergeht","ehrenkranz","schwankende","hinweggeschwunden","gewesengrinsest","zusammengarstig","prolific","forgetting","pancreas","mingles","inaccuracies","phnomenagravitation","kidneycoryza","victoria","advocate","robingrooms","enquiries","unequalled","newcrown","sconecoronation","effected","birmingham","mechanical","necrowned","moneygeorge","nkyi","nky","nki","nukkui","nkee","nkee","vaikeat","nukkui","nukkua","nkyi","nukkui","nki","nkyy","nukkua","nky","nkyy","nkee","nukkuu","varjot","nokea","nokea","nky","nukkuu","nukkua","nki","nukkuu","nyki","vuoksi","vaikuttivat","virkatoimistaan","kvijit","varjot","ingrid","helsinki","nkymtt","punaisenakrapu","sepittnytkirjoituspyt","strhlmaniinjuriidisia","vigle","akvogutantaj","fingroingon","incorporated","enigata","proponikronon","dukinakuirejo","vicarious","increment","thinking","mechanical","nowcrouch","heinecurses","satisfactory","dovecote","dango","mechanical","junequarters","victory","overcoats","bringing","boatswaingreene","cronycarpenter","colonyjersey","scientific","vegetarianism","unchristianlike","singleheart","pinnacle","johnstonegriffith","hawthornegarth","fugg","vacche","fugg","fugg","nuca","nego","nego","figura","invecchiato","varcato","viaggiatori","incresciuto","distinguer","cronaca","dottrinacristiana","facevanoguerre","diconogiorno","fact1","ungrudgingly","conquests","chimneycracks","tinycorner","onegermans","vagabondia","ringrungring","songo","pinnacled","brandnewgarments","classification","increase","noncommissioned","pronecrouching","no4corporals","thinking","nuque","figue","veux","factie","lingratitude","encore","nacre","monotonecrime","lansquenetgarnison","couronnjarres","nkyy","nakkaa","vaikk","viikate","nykk","nukku","nahka","nkee","nky","vaik","nokka","viijet","nukkua","kanervikkoa","viikate","verkkotelineet","viijet","vrjtten","kankahilta","praasniekoilla","kuusikkoinakorpia","perfected","distinguished","journeycrossing","medicinecarreldakin","postponedgiron","victory","provocative","single","destinygreat","lenaquarrel","difficulties","unchristian","uncomfortable","pannikin","volcanocrater","journeycarry","folksongs","aringing","snakebite","staminacourse","vicar","hungry","incumbent","ponycarriage","fix","engraving","longacre","necker","carolinacreditors","winecarriages","sardiniagermany","focus","fingers","mechanically","nogirl","navigator","navigator","encounter","carolinagrateful","neig","laffection","fugitif","lingratitude","sincre","nacquiert","haleinecrainte","soignegurir","monotonesjournellement","navigator","angrily","engagement","mechanical","fannygrowled","moneyguardian","furcoated","concretestrangled","anklebones","spinecurving","explainedgrubby","anygirl","shinyjar","effective","navigator","lincolns","undishonoredgrant","knowcourse","navigator","navigator","monongahela","bonnecampss","boonegreen","abandoningcargo","significantly","angrily","handkerchief","skinnygriefs","knowguarantee","longer","moneycredit","alonecourts","nk","nkyy","nki","nkyy","vaikeat","nukkui","nkee","nky","nky","nky","nyky","nkyi","nkee","nki","nyky","nkee","naukki","vihkot","niukkaa","nkyy","vaikka","vaikutuksettansa","virkatoimien","piispainkronikkaa","helsinki","nykyisemmst","augustinuksenkrysostomon","juteinikaarle","tieteellinenjrjestely","satisfactory","ungrateful","angle","knuckles","opportunitygrinning","nervousnesscareful","anyoneohjourney","avec","annoncrent","singulire","menace","maisonchristine","sonnacoururent","dovecote","hungry","thinkand","newcomers","nowkravetz","knewkeerful","navigator","forgetting","hungry","strangle","binoculars","anywaygrew","consigneecargo","ivike","doing","knuckle","companygrandiloquently","worthlessnesscharacter","diese","mutest","keine","syphax","hoffnungneue","karamasoffanna","modifications","negotiated","goronwygronwy","fekete","fekete","fejt","vegyt","neki","nki","frjt","nyaka","fekdt","vgott","neki","fejt","neki","fejt","neki","neki","neki","vg","fogait","fejet","vgett","nyg","fejt","fakadt","vgyat","fogott","fogad","fik","fk","nyg","neki","vget","vgyott","foglalja","elfogyott","vergdnek","fejt","rovargyjtemnyt","gangra","singer","nekem","elaludnigrfi","itlnikrdik","fergit","fergit","provocative","provocative","fergit","languidlyan","neckan","nowcry","sydneycarton","na2co3","manufacture","levigated","wolvercote","single","technical","chinagrass","figit","increase","mankind","secretknowngrandmothers","conaucorreas","polignyjura","fortifications","overcoats","bankruptcy","nowgrave","nowcourse","dovecote","dovecote","hungry","uncle","prefixed","lincolnsinn","alberoniscard","banquo","donica","advocate","heavyjawed","uncomfortable","abednego","minecried","christianitycarnal","nojourney","victorious","avocat","drawingroomit","commingled","mechanical","pyrogenousgranite","nowcarried","continuesure","hangin","knowgracious","vocation","vegetationanimals","congregated","journeyencampmentdivision","devotionalgravity","newguardianstory","feig","amphiktyonen","gegengru","dunkel","dagegengrlichewo","vaguely","congregate","drunkard","friendsnotcross","nowcared","hungry","encountered","monikins","tetoncredit","mancareless","perfectionism","lincoln","nichols","naygreatest","unworthinessqueer","selfrenunciationsure","nkyi","nki","nahka","nyky","nkyi","varkaat","nahkaa","nukkui","nkyy","nki","nkee","nukkua","nukkui","nkyy","nkyy","nukkuu","nkee","nki","kuvauksia","vaikutuksen","varkaudet","varjottomaksi","kuitenkin","nkynyt","sinkoreita","nuque","neig","vioc","victor","vgtation","fugitifs","lengrenage","cinquimele","uniques","linfinicraindre","soutenucordes","inconnujaurais","earthencrusted","sinking","negatives","sphinginaegroup","carolinachoerocampinae","revoked","vegetablesyes","ungratefully","tongues","laconically","donecrowded","voyageait","vcu","survcut","trangre","inconvnients","uniquement","pianocriant","dauvergnecolonel","faketeach","increase","single","mechanical","tonecreated","enliveninggarb","victories","lincoln","inaugurated","actiongreater","examiningcorrespondence","carolinageorgia","ungrateful","encourage","chronically","destinyicursed","bringing","unaccustomed","semilunarcrescentic","perfectly","ingrain","uncomfortable","monogamy","champagnecream","necka","hungry","incommunicable","inequalities","immediatenessgrowling","againqueried","fugat","navigators416","fingers","snakes","endemoniadacruel","antiphonalchorus","onejourney","foaek","effective","vacatia","thinkin","vernacular","viennacorrespondence","terrific","orkneyinga","hackneycoach","aboynecoheirs","femininegermans","effects","advocate","increase","mankind","concernedgrosser","hungry","hanging","mechanically","tinycrank","imperfections","forgetful","single","inaugural","indianaground","colonygeorgia","avec","lancrenon","rencontr","chronique","genouxcrapaud","cabotinaitquartier","harmoniejournal","fidgeting","hungry","lancasters","unaccountable","gonecardparties","nekaii","fugitivesenmity","arenagreat","babylonianscured","deuteronomyjeremijahu","defection","hungover","foecunda","forgottenecclesiasticus","enquire","tinycraft","stone13quarries","annegermans","fourquet","fourquet","ernecourt","anciennegrote","sultanakharezmehadeeyah","caledoniangerman","knokke","shonecrammd","kenecourtepy","figurehead","kneecurly","ingress","anthonycreagh","werenacarefu","angouleme","venicreator","madeleinecarved","narbonnejordan","forgete","forghete","nekke","forghete","forghete","galyngale","onicle","mongreet","schenegher","dressingroom","incomparably","balconycrowd","knowcarthorse","drawingrooms","bronchitis","johnstonegrumpy","monacaird","onejar","uncommon","examinationascurry","weddingring","snowcountry","wilhelminagrant","evergets","bankinghouse","technically","nowgraver","agostinocaracci","unromanizegermanicus","provocation","ingress","plunket","pinegroves","nocolonel","fhooked","negation","accompaniedgratifying","macaroni89quarrel","journeyjourneying","companycharybdis","nojury","physiological","12","act","number","orthoepy","doubtfulmonotones","timesknow","feug","encroach","condemnedcradle","consciousnesscharacter","gratification","inconceivably","accompanimentscrossing","synchronized","faunacrawled","newgeorgia"
        };
    private string[] removeTheseFromWhitelist;

    public TextAsset RemoveFromWhitelistTextFile; // Drag your text file here in inspector

    public bool debuggingSlurDetector = false;


    public List<string> slurReports = new List<string>();

    private void Start()
    {

        if (RemoveFromWhitelistTextFile != null)
        {
            string wholeText = RemoveFromWhitelistTextFile.text; // Read the content as one big string
            removeTheseFromWhitelist = wholeText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries); // Split by new line
        }

    }





    // call slurCheck with topic or scene, returns bool
    public string RemoveSlurs(string input)
    {

        string currentString = input;
        string metaph = meta3PhonicateVowels(currentString);
        if (debuggingSlurDetector) Debug.Log(metaph);

        int numberOfSlursDetected = 0;
        List<string> removedWords = new List<string>();


        while (ContainsSlur(currentString))
        {

            int startingIndexOfSlur = 0;

            numberOfSlursDetected += 1;
            // if it contains a slur find the part of the input that contains the slur. 



            string slurContainingInput = currentString;

            // start removing 20 charcters at a time.
            while (slurContainingInput.Length > 20 && ContainsSlur(slurContainingInput.Substring(20)))
            {
                startingIndexOfSlur += 20;
                slurContainingInput = slurContainingInput.Substring(20);
                // Debug.Log("checking: " + slurContainingInput);
            }


            // keep removing first letter until there is no slur
            while (ContainsSlur(slurContainingInput.Substring(1)))
            {
                startingIndexOfSlur += 1;
                slurContainingInput = slurContainingInput.Substring(1);
                // Debug.Log("checking: " + slurContainingInput);
            }



            // start removing 20 charcters at a time.           
            while (slurContainingInput.Length > 21 && ContainsSlur(slurContainingInput.Substring(0, slurContainingInput.Length - 20)))
            {
                slurContainingInput = slurContainingInput.Substring(0, slurContainingInput.Length - 20);
                // Debug.Log("checking: " + slurContainingInput);
            }

            // keep removing last letter until there is no slur            
            while (ContainsSlur(slurContainingInput.Substring(0, slurContainingInput.Length - 1)))
            {
                slurContainingInput = slurContainingInput.Substring(0, slurContainingInput.Length - 1);
                // Debug.Log("checking: " + slurContainingInput);
            }




            // slur containing input should now be the minimum string that contains that slur.
            if (debuggingSlurDetector) Debug.Log("found: " + slurContainingInput + " which is: " + meta3PhonicateVowels(slurContainingInput));
            if (debuggingSlurDetector) Debug.Log("current string: " + currentString);


            // get the context of the string which is a slur, so get the 5 characters before and after 
            int startingOfContext = Mathf.Max(startingIndexOfSlur - 5, 0);
            int finishOfContext = Mathf.Min(startingIndexOfSlur + slurContainingInput.Length + 5, currentString.Length);

            string contextString = currentString.Substring(startingOfContext, finishOfContext - startingOfContext);
            string slurReport = "Slur found: " + slurContainingInput + "\tPhonicated: " + meta3PhonicateVowels(slurContainingInput) + "   \tContext: " + contextString;
            slurReports.Add(slurReport);


            if (slurReports.Count > 5)
            {
                try
                {
                    // Get the current date and time	
                    string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    // Create the full path for the file	
                    string path = $"{Environment.CurrentDirectory}\\Assets\\Example Scripts\\SlurReports\\{dateTimeString}_slurReport.txt";
                    // Create an empty file and close it immediately	
                    using (FileStream fs = File.Create(path))
                    {
                        // Close the file immediately to allow subsequent write operations	
                    }

                    string slurReportsAsSingleString = string.Join("\n", slurReports);
                    // Write the string to the file	
                    File.WriteAllText(path, slurReportsAsSingleString);
                    // Log success	
                    Debug.Log("Data saved successfully to: " + path);
                }
                catch (System.Exception e)
                {
                    // Log any exceptions that occur	
                    Debug.LogError("An error occurred while saving data: " + e.Message);
                }


                slurReports = new List<string>();

            }



            // string slurContainingInputRegexSafe = Regex.Escape(slurContainingInput);
            currentString = currentString.Replace(slurContainingInput, "nope");


            // currentString = Regex.Replace(currentString, slurContainingInputRegexSafe, "ppp");
            removedWords.Add(slurContainingInput);




            if (numberOfSlursDetected > 100)
            {
                if (debuggingSlurDetector) Debug.Log("oh fuck");
                break;
            }



        }


        if (debuggingSlurDetector) Debug.Log("number of slurs: " + numberOfSlursDetected);

        foreach (string removedWord in removedWords)
        {
            if (debuggingSlurDetector) Debug.Log("removed word: " + removedWord);
        }


        if (debuggingSlurDetector) Debug.Log("final output: " + currentString);
        if (debuggingSlurDetector) Debug.Log("final output: " + meta3PhonicateVowels(currentString));

        return currentString;
    }

    public bool ContainsSlur(string englishWords)
    {
        string tempString = englishWords.ToLower();
        //find all cases of whitelist words and replace them with you good.
        foreach (string whiteListString in whiteList)
        {
            if (!Array.Exists(removeTheseFromWhitelist, element => element == whiteListString))
            {
                tempString = tempString.Replace(whiteListString.ToLower(), "ppp");
            }
        }
        string meta = meta3PhonicateVowels(tempString);
        foreach (var slur in KnownSlursPhonetic)
        {
            if (meta.Contains(slur))
            {
                return true;
            }
        }

        return false;
    }

    private List<string> KnownSlursPhonetic = new List<string>
        {
            "FAKAT",
            "FARKAT",

            "FAJAT",
            "FARJAT",

            "NKR",
            "NKA",
            "NAKR",
            "NAKKR",
            "NAKA",
            "NKKA",
            "NKKR",
            "NAKKA",

            "NJR",
            "NJA",
            "NAJA",
            "NAJR",
            "NAJJR",
            "NJJA",
            "NJJR",
            "NAJJA"
        };

    public string meta3PhonicateVowels(string input)
    {
        var mV3 = new Metaphone3();

        string[] words = input.Split(' ');
        List<string> metaphoneVowelPhoneticRepresentations = new List<string>();

        foreach (var word in words)
        {
            mV3.SetWord(word);
            mV3.SetEncodeVowels(true);
            mV3.Encode();
            metaphoneVowelPhoneticRepresentations.Add(mV3.GetMetaph());
        }

        return string.Join("", metaphoneVowelPhoneticRepresentations);
    }
}
