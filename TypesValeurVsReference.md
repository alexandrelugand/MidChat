# Types par Valeur vs Types par Référence en C#

## Introduction

En C#, les types de données sont divisés en deux catégories principales : les **types par valeur** (value types) et les **types par référence** (reference types). Cette distinction fondamentale affecte la façon dont les données sont stockées en mémoire et comment elles se comportent lors des assignations et des passages de paramètres.

---

## Types par Valeur (Value Types)

### Définition

Les types par valeur stockent directement leurs données. Lorsqu'une variable de type valeur est assignée à une autre, une **copie complète** des données est créée.

### Caractéristiques

- **Stockage** : Sur la pile (stack) dans la plupart des cas
- **Copie** : Chaque variable contient sa propre copie des données
- **Valeur par défaut** : Initialisé automatiquement (0, false, etc.)
- **Héritage** : Ne peut pas hériter d'autres types (sauf de `System.ValueType`)

### Types par valeur courants

```csharp
// Types numériques
int i = 10;
float f = 20.5f;
double d = 100.10;
decimal price = 99.99m;

// Autres types par valeur
bool isActive = true;
char letter = 'A';

// Structures (struct)
DateTime date = DateTime.Now;
TimeSpan duration = TimeSpan.FromMinutes(30);

// Structures personnalisées
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

### Comportement lors de l'assignation

```csharp
int a = 5;
int b = a;  // b obtient une COPIE de la valeur de a
b = 10;     // Modifie seulement b

Console.WriteLine(a); // Affiche 5
Console.WriteLine(b); // Affiche 10
```

### Passage de paramètres

```csharp
void ModifyValue(int x)
{
    x = 100; // Modifie seulement la copie locale
}

int value = 50;
ModifyValue(value);
Console.WriteLine(value); // Affiche toujours 50

// Avec le mot-clé 'ref' pour passer par référence
void ModifyValueByRef(ref int x)
{
    x = 100; // Modifie la variable originale
}

ModifyValueByRef(ref value);
Console.WriteLine(value); // Affiche maintenant 100
```

---

## Types par Référence (Reference Types)

### Définition

Les types par référence stockent une **référence** (pointeur) vers l'emplacement mémoire où se trouvent les données réelles. Lorsqu'une variable de type référence est assignée à une autre, seule la référence est copiée, pas les données.

### Caractéristiques

- **Stockage** : Les données sont sur le tas (heap), la référence sur la pile
- **Copie** : Les variables partagent la même référence aux données
- **Valeur par défaut** : `null` (pas de référence)
- **Héritage** : Supporte l'héritage

### Types par référence courants

```csharp
// String (cas spécial - immutable)
string str = "Hello, World!";

// Classes
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Tableaux
int[] numbers = new int[] { 1, 2, 3, 4, 5 };

// Delegates
Action myAction = () => Console.WriteLine("Action");

// Interfaces (référencent des implémentations)
IEnumerable<int> collection = new List<int>();
```

### Comportement lors de l'assignation

```csharp
Person person1 = new Person { Name = "Alice", Age = 25 };
Person person2 = person1; // person2 référence le MÊME objet que person1

person2.Name = "Bob";

Console.WriteLine(person1.Name); // Affiche "Bob"
Console.WriteLine(person2.Name); // Affiche "Bob"
// Les deux variables pointent vers le même objet !
```

### Passage de paramètres

```csharp
void ModifyPerson(Person p)
{
    p.Name = "Modified"; // Modifie l'objet original
}

Person myPerson = new Person { Name = "Original" };
ModifyPerson(myPerson);
Console.WriteLine(myPerson.Name); // Affiche "Modified"

// Réassigner la référence (sans 'ref', n'affecte pas l'original)
void ReassignPerson(Person p)
{
    p = new Person { Name = "New Person" }; // Crée un nouvel objet local
}

ReassignPerson(myPerson);
Console.WriteLine(myPerson.Name); // Affiche toujours "Modified"

// Avec 'ref' pour modifier la référence elle-même
void ReassignPersonByRef(ref Person p)
{
    p = new Person { Name = "New Person" }; // Remplace la référence originale
}

ReassignPersonByRef(ref myPerson);
Console.WriteLine(myPerson.Name); // Affiche "New Person"
```

---

## Tableau Comparatif

| Aspect | Types par Valeur | Types par Référence |
|--------|-----------------|-------------------|
| **Stockage** | Pile (stack) | Tas (heap) |
| **Assignation** | Copie complète des données | Copie de la référence |
| **Valeur par défaut** | 0, false, etc. | `null` |
| **Héritage** | Non (sauf ValueType) | Oui |
| **Performance** | Plus rapide pour petites données | Allocation plus lente |
| **Exemples** | `int`, `float`, `bool`, `struct`, `enum` | `class`, `string`, `array`, `delegate`, `interface` |
| **Mot-clé** | `struct`, `enum` | `class`, `interface`, `delegate` |
| **Nullable** | Nécessite `?` (ex: `int?`) | Naturellement nullable |

---

## Cas Spéciaux

### String - Un type référence immutable

```csharp
string s1 = "Hello";
string s2 = s1;
s2 = "World"; // Crée une NOUVELLE chaîne, ne modifie pas s1

Console.WriteLine(s1); // Affiche "Hello"
Console.WriteLine(s2); // Affiche "World"
```

Bien que `string` soit un type par référence, son comportement **immutable** le fait agir comme un type par valeur lors des modifications.

### Boxing et Unboxing

Conversion entre types par valeur et référence :

```csharp
// Boxing : conversion d'un type valeur en type référence
int valueType = 123;
object referenceType = valueType; // Boxing

// Unboxing : conversion d'un type référence en type valeur
int unboxedValue = (int)referenceType; // Unboxing
```

### Types Nullable

Les types par valeur peuvent être rendus nullable avec `?` :

```csharp
int? nullableInt = null; // Type valeur nullable
int regularInt = 0;      // Type valeur normal (ne peut pas être null)

if (nullableInt.HasValue)
{
    Console.WriteLine(nullableInt.Value);
}
```

---

## Recommandations

### Utiliser un struct (type valeur) quand :

- La taille des données est petite (généralement < 16 bytes)
- Le type représente une valeur simple et immutable
- Les instances ne seront pas fréquemment assignées ou passées en paramètre
- Exemples : `Point`, `Color`, `Rectangle`

### Utiliser une class (type référence) quand :

- Les données sont volumineuses
- Le type nécessite de l'héritage ou du polymorphisme
- Le type doit être partagé et modifié par plusieurs références
- Exemples : `Person`, `Order`, `Repository`

---

## Mots-clés Importants

### `ref` - Passage par référence

Permet de passer un type par valeur en référence, ou de modifier la référence d'un type référence :

```csharp
void Function1(ref float f)
{
    f = 10000; // Modifie la variable originale
}

float myFloat = 20.5f;
Function1(ref myFloat);
Console.WriteLine(myFloat); // Affiche 10000
```

### `out` - Paramètre de sortie

Similaire à `ref`, mais ne nécessite pas d'initialisation préalable :

```csharp
bool TryParse(string input, out int result)
{
    result = 0; // Doit être assigné dans la méthode
    return int.TryParse(input, out result);
}
```

### `in` - Passage par référence en lecture seule

Passe par référence mais empêche les modifications (optimisation) :

```csharp
void ProcessLargeStruct(in LargeStruct data)
{
    // data ne peut pas être modifié
    Console.WriteLine(data.Value);
}
```

---

## Conclusion

Comprendre la différence entre types par valeur et types par référence est essentiel pour :
- Écrire du code performant
- Éviter des bugs liés aux références partagées
- Choisir la bonne structure de données pour chaque situation
- Gérer correctement la mémoire en C#

La règle générale : **les types par valeur copient les données, les types par référence partagent les données**.
