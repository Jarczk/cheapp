# Sprawozdanie z projektu "Cheapp"
 
## Wprowadzenie
 
Celem niniejszego sprawozdania jest przedstawienie architektury oraz sposobu dzialania aplikacji "Cheapp". Projekt powstal jako narzędzie do agregowania ofert promocyjnych i kuponów z różnych sklepów. Poza wyszukiwaniem promocji umożliwia on zarządzanie ulubionymi produktami oraz historią wyszukiwań. W ramach repozytorium znajdują się trzy główne komponenty:
 
1. **Backend** zbudowany w technologii .NET
2. **Frontend** oparty na Next.js i TypeScript
3. **Baza danych** MongoDB
 
W kolejnych sekcjach opisane zostaną poszczególne elementy systemu oraz ich współdziałanie.
 
## Struktura projektu
 
Repozytorium ma strukturę uproszczonego monorepo:
 
```
cheapp/
├─ server/        # backend .NET Web API
├─ client3/       # frontend w Next.js
├─ README.md
└─ sprawozdanie.md
```
 
### Backend (`server/Cheapp`)
 
Backend aplikacji został napisany w C# z wykorzystaniem ASP.NET Web API. Zawiera logikę biznesową związaną z obsługą użytkowników, wyszukiwaniem ofert w zewnętrznych serwisach (np. eBay), przechowywaniem informacji w MongoDB oraz zarządzaniem sesjami czatu.
 
### Frontend (`client3`)
 
Warstwa kliencka to projekt w Next.js (React) napisany w TypeScript. Odpowiada za interakcję z użytkownikiem – prezentację listy produktów, logowanie, rejestrację oraz komunikację z API. Korzysta z biblioteki Zustand do przechowywania stanu uwierzytelnienia i z Tailwind CSS do stylowania.
 
### Baza danych
 
Do przechowywania danych użyto MongoDB, które pozwala w elastyczny sposób gromadzić kolekcje dokumentów (np. `Favorites`, `searchhistory`, `Users`). Dostęp do bazy w backendzie realizowany jest poprzez oficjalny sterownik `MongoDB.Driver`.
 
## Backend
 
### Konfiguracja
 
Plik `appsettings.json` definiuje podstawowe opcje konfiguracyjne, m.in.:
 
```json
{
  "Ebay": {
    "BaseUrl": "https://api.sandbox.ebay.com/buy/browse/v1/",
    "Marketplace": "EBAY_US"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017/Cheapp",
    "DatabaseName": "Cheapp"
  },
  "Jwt": {
    "Issuer": "cheappApi",
    "Audience": "cheappClient"
  }
}
```
 
Aplikacja korzysta z JWT do autoryzacji – klucz i parametry tokenu konfigurowane są w sekcji `Jwt`. Dodatkowo definiowane są ustawienia dla integracji z eBay i ewentualnego klienta LLM (Groq).
 
### Najważniejsze kontrolery
 
1. **AuthController** – odpowiada za rejestrację, logowanie oraz zwracanie informacji o aktualnie zalogowanym użytkowniku. Dane użytkowników są przechowywane w kolekcji `Users` w MongoDB przy użyciu bibliotek `AspNetCore.Identity.Mongo`.

  Poniżej przykład tworzenia konta użytkownika:

  ![image](https://github.com/user-attachments/assets/37e97af0-1cab-41f6-a761-5e407e2703b4)

  Przykład rekordu z kolekcji 'Users' po stworzeniu konta:

  ![image](https://github.com/user-attachments/assets/7302ad33-251c-48f3-b0eb-38cf2e133f29)

  Po stworzeniu konta użytkownik może się zalogować:

  ![image](https://github.com/user-attachments/assets/b6dfb6e8-fd6b-421a-a7fa-bef0688e4d71)


3. **OffersController** – udostępnia endpoint `GET /api/offers` do wyszukiwania ofert w serwisie eBay. Zwraca posortowaną listę produktów według ceny, a wynik zapisywany jest w historii wyszukiwań danego użytkownika.

  Przykład wyszukiwania produktu:
  
  ![image](https://github.com/user-attachments/assets/eaee4815-8665-4518-a7f7-fb54edef158e)
  
  Zwrócona lista posortowanych produktów:
  
  ![image](https://github.com/user-attachments/assets/24075953-2a57-41b0-b1ab-3604b7a527cd)


5. **FavoritesController** – pozwala dodawać i usuwać produkty z listy ulubionych, a także pobierać listę ulubionych ofert. Każda operacja powiązana jest z identyfikatorem użytkownika pobranym z tokena JWT.

  Po dodaniu produktu do ulubionych zostaje on oznaczony:
  
  ![image](https://github.com/user-attachments/assets/936e8453-3eab-4422-a9da-ca49946890f9)
  
  Można go następnie znaleźć w zakładce 'Favorites':
  
  ![image](https://github.com/user-attachments/assets/b2611b17-0419-48de-829f-561142daecb8)
  
  Przykład rekordu z kolekcji 'Favorites' z MongoDBCompass:
  
  ![image](https://github.com/user-attachments/assets/7d58c16d-7a8e-4fb4-9853-f02b4c012e00)


7. **SearchHistoryController** – umożliwia pobieranie i czyszczenie historii wyszukiwań. Dla jednego użytkownika przechowywanych jest maksymalnie 50 ostatnich wyszukiwań.

  Przykład zapisanej historii wyszukiwania:
  
  ![image](https://github.com/user-attachments/assets/5a6b9c14-b8ea-4978-9699-f1ac9bc4b762)

9. **ProductsController** – udostępnia endpoint do pobierania informacji o pojedynczym produkcie na podstawie jego identyfikatora.

  Przykład wyświetlenia pojedyńczego produktu:
  
  ![image](https://github.com/user-attachments/assets/b4d9357a-e4ce-4473-ac1b-81453b81946b)

### Warstwa usług
 
W folderze `Services` znajdują się klasy implementujące logikę biznesową:
 
- `EbayClient` – integruje się z API eBay, wyszukując produkty lub pobierając pełne dane o danej ofercie. Zwracane obiekty są mapowane na wewnętrzny model `Offer`.
- `OfferAggregator` – agreguje oferty z różnych źródeł (obecnie z eBay) i zwraca maksymalnie 10 najtańszych wyników.
- `MongoFavoritesService` – zarządza kolekcją `Favorites` w MongoDB. Odpowiada za dodawanie oraz usuwanie ulubionych produktów i tworzenie indeksów przyspieszających zapytania.
- `SearchHistoryService` – obsługuje kolekcję `searchhistory`, zapisując każde wyszukiwanie wraz z liczbą wyników oraz przechowując jedynie 50 najnowszych rekordów na użytkownika.
 
### Modele danych
 
Przykładowy model `Favorite` prezentuje się następująco:
 
```csharp
public class Favorite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
 
    public string UserId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    [BsonIgnoreIfDefault]
    public string Notes { get; set; } = string.Empty;
}
```
 
Analogicznie zdefiniowane są modele `SearchHistory` oraz `Offer`. Wszystkie kolekcje w MongoDB posiadają indeksy, co usprawnia wyszukiwanie po `UserId`.
 
### Przebieg zapytań
 
1. Użytkownik wysyła zapytanie `GET /api/offers?q=...`.
2. Backend za pomocą `EbayClient` pobiera listę ofert z API eBay.
3. `OfferAggregator` sortuje wyniki rosnąco po cenie i zwraca 10 najtańszych ofert.
4. Jeśli użytkownik jest zalogowany, zapytanie jest zapisywane w `searchhistory` poprzez `SearchHistoryService`.
5. Odpowiedź trafia do frontendu w formacie JSON.
 
## Frontend
 
### Podstawowe założenia
 
Aplikacja kliencka została utworzona w Next.js i wykorzystuje nowy system folderów `app/`. Poszczególne strony dostępne są pod następującymi ścieżkami:
 
- `/` – strona główna z możliwością wyszukiwania produktów.
- `/products` – prezentacja wyników wyszukiwania.
- `/auth/login` oraz `/auth/register` – logowanie i rejestracja.
- `/favorites` – lista ulubionych produktów.
- `/profile` – profil użytkownika z historią wyszukiwań.
 
Za komunikację z backendem odpowiada moduł `lib/api.ts`, w którym zdefiniowane są funkcje wysyłające zapytania HTTP. Przykład użycia metody wyszukiwania produktów:
 
```typescript
async searchProducts(query: string, page: number = 1): Promise<Product[]> {
  return this.request<Product[]>(`/offers?q=${encodeURIComponent(query)}&page=${page}`)
}
```
 
### Obsługa stanu
 
Stan logowania przechowywany jest w `useAuthStore` (biblioteka Zustand). Po zalogowaniu token JWT zapisywany jest w `localStorage`, a kolejne zapytania do API przekazują go w nagłówku `Authorization`.
 
```typescript
export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      isAuthenticated: false,
      setAuth: (token: string) => {
        setAuthToken(token)
        set({ token, isAuthenticated: true })
      },
      logout: () => {
        removeAuthToken()
        set({ token: null, isAuthenticated: false })
      },
      initialize: () => {
        const token = getAuthToken()
        if (token) {
          set({ token, isAuthenticated: true })
        }
      },
    }),
    { name: 'auth-storage' }
  )
)
```
 
### Wybrane komponenty
 
- Strona główna (`app/page.tsx`) zawiera formularz wyszukiwania i przedstawia kluczowe funkcje aplikacji w formie kart z ikonami.

  ![image](https://github.com/user-attachments/assets/6a19b7de-d7c4-469c-8e2a-4dba59efb328)

- Widok produktów (`app/products`) pobiera dane z backendu i wyświetla je w siatce kart. Można tutaj dodać wybrany produkt do ulubionych.

  ![image](https://github.com/user-attachments/assets/f71d2668-9feb-42cc-aba6-54e41f77939f)

- Sekcja profilu (`app/profile`) umożliwia przeglądnięcie historii wyszukiwań pobieranej z API `SearchHistoryController` oraz zarządzanie ulubionymi produktami.

  ![image](https://github.com/user-attachments/assets/1b9b4886-89e9-44ce-b394-444c3e171f6d)

### Stylowanie i interakcje
 
Stylowanie realizowane jest przez Tailwind CSS oraz gotowe komponenty UI. Interakcje (dodawanie do ulubionych, logowanie, paginacja wyników) obsługiwane są w całości po stronie klienta, a następnie delegowane do backendu.
 
## Baza danych – MongoDB
 
Dane przechowywane są w dokumentowej bazie MongoDB. Główne kolekcje to:
 
- `Users` i `Roles` – tworzone przez bibliotekę Identity do obsługi rejestracji i logowania.
- `Favorites` – przechowuje powiązania użytkowników z produktami dodanymi do ulubionych. Dokument zawiera identyfikator produktu, datę dodania oraz ewentualne notatki.
- `searchhistory` – zapisuje historię wyszukiwań wraz z czasem i liczbą wyników.
 
Operacje na bazie wykonuje `MongoFavoritesService` oraz `SearchHistoryService`. Oba serwisy inicjalizują indeksy w kolekcjach (np. unikalne połączenie `UserId + ProductId` w `Favorites`), co zwiększa szybkość zapytań i zapobiega duplikatom.
  
  Przykład struktury danych:
  
  ![image](https://github.com/user-attachments/assets/95b6841b-10b1-419e-a048-ab9fd518149f)

### Przykładowy zapis w kolekcji `Favorites`
 
```json
{
  "_id": "66123456789abcdef0123456",
  "userId": "12345",
  "productId": "98765",
  "addedAt": "2024-05-01T12:00:00Z",
  "notes": "Przykładowa notatka"
}
```
 
### Zalety wykorzystania MongoDB
 
- Elastyczny schemat dokumentów pozwala łatwo rozszerzać strukturę danych.
- Indeksy na polach `userId` oraz `productId` przyspieszają wyszukiwanie i zapobiegają duplikatom w ulubionych.
- Łatwe skalowanie i możliwość hostowania zarówno lokalnie, jak i w chmurze.
 
## Współdziałanie komponentów
 
1. Użytkownik wprowadza zapytanie w interfejsie WWW.
2. Frontend wysyła żądanie do `GET /api/offers` w backendzie.
3. Backend pobiera wyniki z eBay, sortuje i zwraca maksymalnie 10 najtańszych ofert.
4. W razie potrzeby zapytanie jest logowane w kolekcji `searchhistory`.
5. Użytkownik może oznaczyć produkty jako ulubione, co wywołuje `POST /api/favorites` i zapis w bazie.
6. Informacje o zalogowanym użytkowniku pobierane są z tokena JWT, przekazywanego w nagłówku każdej prośby.
 
## Podsumowanie
 
Aplikacja "Cheapp" stanowi kompletny system agregujący oferty z zewnętrznych serwisów i udostępniający je użytkownikom w przyjaznej formie. Backend w .NET zajmuje się obsługą logiki biznesowej, autoryzacji oraz komunikacji z MongoDB. Frontend w Next.js zapewnia czytelny interfejs użytkownika i umożliwia wygodne korzystanie z funkcji aplikacji. Dzięki wykorzystaniu bazy MongoDB możliwe jest łatwe skalowanie oraz przechowywanie danych w elastyczny sposób.
 
