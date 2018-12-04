// -- Start: Our Application Code --

class MathBoo {
 add (a: number, b: number) {
    return a + b;
  }
  
   subtract (a: number, b: number) {
    return a - b;
  }
  
   divide (a: number, b: number) {
    return a / b;
  }
  
   multiply (a: number, b: number) {
    return a * b;
  }
}
  // -- End: Our Application Code --
  
  // -- Start: Our Cypress Tests --
    describe('MathBoo', function() {
        let m = new MathBoo();
        it('SHOULD add GIVEN valid numbers', () => expect(m.add(1, 2)).to.eq(3));    
        it('can subtract numbers', () => expect(m.subtract(5, 12)).to.eq(-7));
    
        specify('can divide numbers', function() {
            expect(m.divide(27, 9)).to.eq(3)
        })
  
      specify('can multiply numbers', function() {
        expect(m.multiply(5, 4)).to.eq(20)
      })
    })
  // -- End: Our Cypress Tests --