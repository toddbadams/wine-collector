import { add } from '../support/add'

describe('TypeScript', () => {
  it('works', () => {
    // note TypeScript definition
    const x: number = 42
  })

  it('checks shape of an object', () => {
    const object = {
      age: 21,
      name: 'Joe',
    }
    expect(object).to.have.all.keys('name', 'age')
  })

  it('uses cy commands', () => {
    cy.wrap({}).should('deep.eq', {})
  })


  // enable once we release updated TypeScript definitions
  it('has Cypress object type definition', () => {
    expect(Cypress.version).to.be.a('string')
  })

  it('adds numbers', () => {
    expect(add(2, 3)).to.equal(5)
  })

  it('uses custom command cy.foo()', () => {
    debugger;
    cy.foo().should('be.equal', 'foo')
  })
})