namespace Treasara.Domain

type Notional =
    { Amount : decimal
      Currency : Currency }

module Notional =

    let create amount currency =
        if amount <= 0m then
            invalidArg "amount" "Notional must be positive."

        { Amount = amount
          Currency = currency }