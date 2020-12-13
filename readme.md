# Supaprastintos blokų grandinės (blockchain) kūrimas

#### Blokų grandinių technologijų kurso 2-oji praktinė užduotis



## Įdiegimo ir naudojimosi instrukcija

- Parsisiųskite naujausią (rekomenduojama) release'o versiją
- Sukompiliuokite ir paleiskite `Program.cs` failą savo norimoje aplinkoje (rekomenduojama MS Visual Studio)



## Programos versijos

v0.1 WIP 

Sukurtos visos pagrindinių klasių struktūros. Generuojami user'iai, transakcijų baseinas bei pridedamos transakcijos pradiniam blokui.

v0.1 

Pilnai padaryta v0.1 užduoties versija

## Naudotos klasių struktūros

###### Block.cs

```c#
class Block
    {
        /* header */
        public string prevHash { get; set; }
        public long timestamp { get; set; }
        public int version { get; set; }
        public string merkelRootHash { get; set; }
        public int nonce { get; set; }
        public string diffTarget { get; set; }

​		/* body */

​		public List<Transaction> TxPool;
​    }
```

###### Transaction.cs

```c#
class Transaction
    {
        public string ID { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public int Amount { get; set; }
    }
```

###### User.cs

```c#
class User
    {
        public string name { get; set; }
        public string hashKey { get; set; }
        public int balance { get; set; }
    }
```



## Kaip veikia programa?

Pagrindinė logika vyksta `Program.cs` faile. 

##### v0.1

- Sugeneruojami pradiniai 1000 naudotojų `generateUsers()` metode.
- Esamiems vartotojams atsitiktine tvarka sugeneruojamos 10000 transakcijų baseinas.
- Pirmajam (genesis) blokui priskiriamos atsitiktinės 100 transakcijų, blokas pridedamas į blockchain'ą.
- Sekantys procesai vyksta tol, kol transakcijų baseine yra neišskirstytų transakcijų:
  - Mininamas blokas. Keičiantis nonce skaičiui, ieškoma hash'o su tinkamu difficulty target'u. Visi bandyti hashai išvedami į failą.
  - Kai surandamas tinkamas hashas, naujajam blokui priskiriami reikiami header'io duomenys (praeito bloko hashas, laiko žyma ir t.t.)
  - Naujajam blokui priskiriamos **tinkamos** transakcijos. Siunčiama 100 kandidatų, tačiau neteisingos transakcijos gali būti atmestos.