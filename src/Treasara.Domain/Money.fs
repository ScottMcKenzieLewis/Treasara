namespace Treasara.Domain

type Money =
    { Amount : decimal
      Currency : Currency }

module Money =

    let create amount currency =
        { Amount = amount
          Currency = currency }

    let add m1 m2 =
        if m1.Currency <> m2.Currency then
            invalidArg "m2" "Cannot add money values with different currencies."

        { m1 with Amount = m1.Amount + m2.Amount }

    let scale factor money =
        { money with Amount = money.Amount * factor }

    let sumSameCurrency (ms: Money list) : Money option =
        match ms with
        | [] -> None
        | first :: rest ->
            let total =
                rest |> List.fold add first
            Some total