﻿namespace Checkout.Domain

// ------------------------------------------------------------------
// Product information

type Code = string
type Name = string
type Price = decimal
type Picture = string
type Quantity = decimal

type Product = Product of Code * Name * Picture * Price

// ------------------------------------------------------------------
// Representing purchase as line items

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelLineItem of int

type LinePurchase = list<LineItem>

// ------------------------------------------------------------------
// Representing aggregated purchase for the bill

type FinalPurchase = Map<Product, Quantity>

// ------------------------------------------------------------------
// Transformation between representations

module Purchase = 

  let processPurchase (line : LinePurchase) : FinalPurchase =
    // Build a list of cancelled items
    let cancelled = line |> Seq.choose (function
        | CancelLineItem id -> Some id | _ -> None) |> set

    // Pick all items that have not been cancelled
    // Group them by product and add quantities in each group
    line 
    |> Seq.choose (function
        | SaleLineItem(id, prod, q) when not(cancelled.Contains(id)) ->
            Some(prod, q)
        | _ -> None)
    |> Seq.groupBy fst
    |> Seq.map (fun (p, pqs) -> 
        p, pqs |> Seq.sumBy snd) 
    |> Map.ofSeq
